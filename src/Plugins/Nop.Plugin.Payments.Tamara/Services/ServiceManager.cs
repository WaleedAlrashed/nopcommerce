﻿using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Http.Extensions;
using Nop.Data;
using Nop.Plugin.Payments.Tamara.Domain;
using Nop.Plugin.Payments.Tamara.Domain.RequestParams;
using Nop.Plugin.Payments.Tamara.Domain.Requests;
using Nop.Plugin.Payments.Tamara.Domain.Responses;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Tax;


namespace Nop.Plugin.Payments.Tamara.Services
{
    public class ServiceManager
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressService _addresService;
        //private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly TamaraHttpClient _tamaraHttpClient;
        private readonly IRepository<TamaraPaymentTransaction> _tamaraPaymentTransactionRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;


        #endregion

        #region Ctor

        public ServiceManager(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addresService,
            //ICheckoutAttributeParser checkoutAttributeParser,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IWorkContext workContext,
            TamaraHttpClient tamaraHttpClient,
            IRepository<TamaraPaymentTransaction> tamaraPaymentTransactionRepository,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addresService = addresService;
            //_checkoutAttributeParser = checkoutAttributeParser;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _workContext = workContext;
            _tamaraHttpClient = tamaraHttpClient;
            _tamaraPaymentTransactionRepository = tamaraPaymentTransactionRepository;
            _localizationService = localizationService;
            _customerService = customerService;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Create an order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderGuid">Order GUID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the created order; error message if exists
        /// </returns>
        public async Task<(OrderResponse Result, string Error)> CreateOrderAsync(TamaraSettings settings, string orderGuid)
        {
            return await HandleFunctionAsync(async () =>
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                Currency currency = new Currency();
                if (customer.CurrencyId.HasValue)
                {
                    currency = (await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId.Value));
                }
                else
                {
                    currency = await _workContext.GetWorkingCurrencyAsync();
                }
                if (currency == null)
                    throw new NopException("Primary store currency not set");

                var shoppingCart = (await _shoppingCartService
                    .GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id))
                    .ToList();

                var shippingAddress = await _addresService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);

                var country = (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;

                //prepare purchase unit details
                var taxTotal = Math.Round((await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCart, false)).taxTotal, 2);
                var (cartShippingTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart, false);
                var shippingTotal = Math.Round(cartShippingTotal ?? decimal.Zero, 2);
                var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService
                    .GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);
                var orderTotal = Math.Round((shoppingCartTotal * currency.Rate) ?? decimal.Zero, 2);

                var request = new CheckoutRequest();
                //request.PaymentType = "PAY_BY_FULL";
                request.PaymentType = "PAY_BY_INSTALMENTS";
                request.CountryCode = country;
                request.OrderReferenceId = orderGuid;
                request.Description = "Nopcommerce ..";
                //Shipping address
                request.ShippingAddress = new ShippingAddressParams(
                    shippingAddress.FirstName,
                    shippingAddress.LastName,
                    shippingAddress.Address1,
                    shippingAddress.City,
                    country);
                //Consumer
                request.Consumer = new ConsumerParams(
                    shippingAddress.FirstName,
                    shippingAddress.LastName,
                    shippingAddress.PhoneNumber,
                    shippingAddress.Email);
                //Items
                foreach (var item in shoppingCart)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        request.Items.Add(
                            new ProductParams(
                                item.Id,
                                "type",
                                product.Name,
                                product.Sku,
                                item.Quantity,
                                new AmountParams((item.Quantity * product.Price), currency.CurrencyCode)
                                )
                            );
                    }
                }

                //Tax
                request.TaxAmount = new AmountParams(taxTotal, currency.CurrencyCode);
                //Total amount
                request.TotalAmount = new AmountParams(orderTotal, currency.CurrencyCode);
                //Shipping amount
                request.ShippingAmount = new AmountParams(shippingTotal, currency.CurrencyCode);

                var currentLanguage = await _workContext.GetWorkingLanguageAsync();
                if (currentLanguage.UniqueSeoCode.Contains("ar"))
                {
                    request.Lang = "ar_SA";
                }
                else
                {
                    request.Lang = "en_US";

                }

                //Merchant Urls
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var baseUri = $"{store.Url.TrimEnd('/')}{urlHelper.RouteUrl(TamaraDefault.WebhookRouteName)}".ToLowerInvariant();
                request.MerchantUrl = new MerchantUrlParams(baseUri);

                var result = await _tamaraHttpClient.RequestAsync<CheckoutRequest, OrderResponse>(request, settings)
                        ?? throw new NopException($"Empty result");

                result.StoreId = store.Id;
                result.CustomerId = customer.Id;
                result.OrderTotal = orderTotal;

                return result;
            });
        }
        public async Task<OrderResponse> TestCreateOrderAsync(TamaraSettings settings, string orderGuid)
        {
            try
            {
                if (settings == null)
                {
                    throw new NullReferenceException("settings is null");
                }
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer == null)
                {
                    throw new NullReferenceException("Customer is null");
                }

                var store = await _storeContext.GetCurrentStoreAsync();
                if (store == null)
                {
                    throw new NullReferenceException("Store is null");
                }

                var currencyId = customer.CurrencyId;
                if (!currencyId.HasValue)
                {
                    throw new NullReferenceException("Customer currency ID is null");
                }

                var currency = await _currencyService.GetCurrencyByIdAsync(currencyId.Value);
                if (currency == null)
                {
                    throw new NullReferenceException("Currency is null");
                }

                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (shoppingCart == null)
                {
                    throw new NullReferenceException("Shopping cart is null");
                }

                var shippingAddressId = customer.ShippingAddressId;
                if (!shippingAddressId.HasValue)
                {
                    throw new NullReferenceException("Customer shipping address ID is null");
                }

                var shippingAddress = await _addresService.GetAddressByIdAsync(shippingAddressId.Value);
                if (shippingAddress == null)
                {
                    throw new NullReferenceException("Shipping address is null");
                }

                var countryId = shippingAddress.CountryId;
                if (!countryId.HasValue)
                {
                    throw new NullReferenceException("Country ID in shipping address is null");
                }

                var country = await _countryService.GetCountryByIdAsync(countryId.Value);
                if (country == null)
                {
                    throw new NullReferenceException("Country is null");
                }

                var request = new CheckoutRequest();
                request.PaymentType = "PAY_BY_FULL";
                request.CountryCode = country.TwoLetterIsoCode;
                request.OrderReferenceId = orderGuid;
                request.Description = "Nopcommerce ..";

                request.ShippingAddress = new ShippingAddressParams(
                    shippingAddress.FirstName,
                    shippingAddress.LastName,
                    shippingAddress.Address1,
                    shippingAddress.City,
                    request.CountryCode);

                request.Consumer = new ConsumerParams(
                    shippingAddress.FirstName,
                    shippingAddress.LastName,
                    shippingAddress.PhoneNumber,
                    shippingAddress.Email);

                foreach (var item in shoppingCart)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        request.Items.Add(
                            new ProductParams(
                                item.Id,
                                "type",
                                product.Name,
                                product.Sku,
                                item.Quantity,
                                new AmountParams((item.Quantity * product.Price), currency.CurrencyCode)
                            )
                        );
                    }
                }
                //prepare purchase unit details
                var taxTotal = Math.Round((await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCart, false)).taxTotal, 2);
                var (cartShippingTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart, false);
                var shippingTotal = Math.Round(cartShippingTotal ?? decimal.Zero, 2);
                var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService
                    .GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);
                var orderTotal = Math.Round((shoppingCartTotal * currency.Rate) ?? decimal.Zero, 2);
                //Tax
                request.TaxAmount = new AmountParams(taxTotal, currency.CurrencyCode);
                //Total amount
                request.TotalAmount = new AmountParams(orderTotal, currency.CurrencyCode);
                //Shipping amount
                request.ShippingAmount = new AmountParams(shippingTotal, currency.CurrencyCode);



                var currentLanguage = await _workContext.GetWorkingLanguageAsync();
                request.Lang = currentLanguage.UniqueSeoCode.Contains("ar") ? "ar_SA" : "en_US";

                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var baseUri = $"{store.Url.TrimEnd('/')}{urlHelper.RouteUrl(TamaraDefault.WebhookRouteName)}".ToLowerInvariant();
                request.MerchantUrl = new MerchantUrlParams(baseUri);

                var result = await _tamaraHttpClient.RequestAsync<CheckoutRequest, OrderResponse>(request, settings);
                if (result == null)
                {
                    throw new NullReferenceException("Tamara API response is null");
                }

                result.StoreId = store.Id;
                result.CustomerId = customer.Id;
                result.OrderTotal = orderTotal;

                return result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        /// <summary>
        /// Capture a previously created order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderId">Order id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the captured order; error message if exists
        /// </returns>

        public async Task<(OrderResponse OrderResponse, string Error)> CaptureAsync(TamaraSettings settings, OrderDetailsResponse order)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                var request = new CaptureRequest();
                request.PaymentType = "PAY_BY_INSTALMENTS";
                request.CountryCode = order.CountryCode;
                request.OrderId = order.OrderId;
                request.Description = "Nopcommerce ..";
                request.ShippingAddress = order.ShippingAddress;
                request.Consumer = order.Consumer;
                request.Items = order.Items;
                request.TaxAmount = order.TaxAmount;
                request.ShippingAmount = order.ShippingAmount;
                request.TotalAmount = order.TotalAmount;

                var result = await _tamaraHttpClient.RequestAsync<CaptureRequest, OrderResponse>(request, settings)
                        ?? throw new NopException($"Empty result");

                return result;
            });
        }

        public static bool IsConfigured(TamaraSettings settings)
        {
            //Api key required to request services
            return !string.IsNullOrEmpty(settings?.ApiToken);
        }

        public async Task<OrderResponse> HandleCheckoutRequestAsync(TamaraSettings settings, string orderReferenceId)
        {

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var currency = (await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId.Value));
            if (currency == null)
                throw new NopException("Primary store currency not set");

            var shoppingCart = (await _shoppingCartService
                .GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id))
                .ToList();

            var shippingAddress = await _addresService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);

            var country = (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;


            var request = new CheckoutRequest();
            request.PaymentType = "PAY_BY_INSTALMENTS";
            request.CountryCode = country;
            request.OrderReferenceId = orderReferenceId;
            request.Description = "Nopcommerce ..";
            //Shipping address
            request.ShippingAddress = new ShippingAddressParams(
                shippingAddress.FirstName,
                shippingAddress.LastName,
                shippingAddress.Address1,
                shippingAddress.City,
                country);
            //Consumer
            request.Consumer = new ConsumerParams(
                shippingAddress.FirstName,
                shippingAddress.LastName,
                shippingAddress.PhoneNumber,
                shippingAddress.Email);
            //Items
            foreach (var item in shoppingCart)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    request.Items.Add(
                        new ProductParams(
                            item.Id,
                            "type",
                            product.Name,
                            product.Sku,
                            item.Quantity,
                            new AmountParams((item.Quantity * product.Price), currency.CurrencyCode)
                            )
                        );
                }
            }

            //prepare purchase unit details
            var taxTotal = Math.Round((await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCart, false)).taxTotal, 2);
            var (cartShippingTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCart, false);
            var shippingTotal = Math.Round(cartShippingTotal ?? decimal.Zero, 2);
            var (shoppingCartTotal, _, _, _, _, _) = await _orderTotalCalculationService
                .GetShoppingCartTotalAsync(shoppingCart, usePaymentMethodAdditionalFee: false);
            var orderTotal = Math.Round((shoppingCartTotal * currency.Rate) ?? decimal.Zero, 2);
            //Tax
            request.TaxAmount = new AmountParams(taxTotal, currency.CurrencyCode);
            //Total amount
            request.TotalAmount = new AmountParams(orderTotal, currency.CurrencyCode);
            //Shipping amount
            request.ShippingAmount = new AmountParams(shippingTotal, currency.CurrencyCode);
            //Merchant Urls
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var baseUri = $"{store.Url.TrimEnd('/')}{urlHelper.RouteUrl(TamaraDefault.WebhookRouteName)}".ToLowerInvariant();
            request.MerchantUrl = new MerchantUrlParams(baseUri);

            var result = await _tamaraHttpClient.RequestAsync<CheckoutRequest, OrderResponse>(request, settings)
                    ?? throw new NopException($"Empty result");
            return result;
        }

        /// <summary>
        /// Authorize a previously created order
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="orderId">Order id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the authorized order; error message if exists
        /// </returns>
        public async Task<(OrderResponse Order, string Error)> AuthorizeAsync(TamaraSettings settings, string orderId)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                var request = new AuthoriseRequest(orderId);
                return await _tamaraHttpClient.RequestAsync<AuthoriseRequest, OrderResponse>(request, settings)
                    ?? throw new NopException($"Empty result");

            });
        }

        /// <summary>
        /// Void an authorization
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="authorizationId">Authorization id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the voided order; Error message if exists
        /// </returns>
        public async Task<(object Order, string Error)> VoidAsync(TamaraSettings settings, string tamaraOrderId, string checkoutId)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                var request = new VoidRequest(tamaraOrderId, checkoutId);
                return await _tamaraHttpClient.RequestAsync<VoidRequest, VoidResponse>(request, settings)
                    ?? throw new NopException($"Empty result");
            });
        }

        /// <summary>
        /// Delete webhook
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteWebhookAsync(TamaraSettings settings)
        {
            await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                if (string.IsNullOrEmpty(settings.WebhookId))
                    return null;

                var (webhook, _) = await GetWebhookAsync(settings);
                if (webhook is null)
                    return null;

                var request = new DeleteWebhookRequest(settings.WebhookId);
                var response = await _tamaraHttpClient.RequestAsync<DeleteWebhookRequest, DeleteWebhookResponse>(request, settings)
                        ?? throw new NopException($"Empty result");

                return response;
            });
        }

        /// <summary>
        /// Get webhook by the URL
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="webhookUrl">Webhook URL</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the webhook; error message if exists
        /// </returns>
        public async Task<(WebhookResponse RetrieveWebhookResponse, string Error)> GetWebhookAsync(TamaraSettings settings)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                var request = new RetrieveWebhookRequest(settings.WebhookId);
                var response = await _tamaraHttpClient.RequestAsync<RetrieveWebhookRequest, WebhookResponse>(request, settings)
                        ?? throw new NopException($"Empty result");
                return response;
            });
        }

        /// <summary>
        /// Create webhook that receive events for the subscribed event types
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="storeId">Store id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the webhook; error message if exists
        /// </returns>
        public async Task<(WebhookResponse RegisterWebhookResponse, string Error)> CreateWebhookAsync(TamaraSettings settings, int storeId)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (!IsConfigured(settings))
                    throw new NopException("Plugin not configured");

                //prepare webhook URL
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                var store = storeId > 0
                    ? await _storeService.GetStoreByIdAsync(storeId)
                    : await _storeContext.GetCurrentStoreAsync();
                var webhookUrl = $"{store.Url.TrimEnd('/')}{urlHelper.RouteUrl(TamaraDefault.WebhookRouteName)}".ToLowerInvariant();

                //check whether the webhook already exists
                if (!string.IsNullOrEmpty(settings.WebhookId))
                {
                    var (webhook, _) = await GetWebhookAsync(settings);
                    if (webhook is not null)
                        return webhook;
                }
                var request = new RegisterWebhookRequest(webhookUrl);
                var response = await _tamaraHttpClient.RequestAsync<RegisterWebhookRequest, WebhookResponse>(request, settings)
                        ?? throw new NopException($"Empty result");
                response.WebhookUrl = webhookUrl;
                return response;
            });
        }

        /// <summary>
        /// Handle webhook request
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="request">HTTP request</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleWebhookAsync(TamaraSettings settings, string tamaraOrderId)
        {
            await HandleFunctionAsync(async () =>
            {
                try
                {
                    bool isFromMobile = false;
                    bool success = false;
                    //ensure that plugin is configured
                    if (!IsConfigured(settings))
                        throw new NopException("Plugin not configured");

                    var (orderDetailResponse, _) = await GetOrderDetailAsync(settings, tamaraOrderId);

                    if (orderDetailResponse is null)
                        return false;

                    if (!Guid.TryParse(orderDetailResponse.OrderReferenceId.ToUpper(), out var orderGuid))
                        throw new NopException($"Could not recognize an order reference '{orderDetailResponse.OrderReferenceId}'");

                    var paymentRequest = _actionContextAccessor.ActionContext.HttpContext.Session
                    .Get<ProcessPaymentRequest>(TamaraDefault.PaymentRequestSessionKey);

                    if (paymentRequest == null)
                    {

                        var tamaraTransaction = await GetTamaraTransactionByOrderReference(orderGuid);
                        if (tamaraTransaction is null)
                        {
                            throw new NopException($"Could not find tamaraTransaction {orderGuid}");
                        }
                        var customer = await _customerService.GetCustomerByIdAsync(customerId: tamaraTransaction.CustomerId);
                        await _workContext.SetCurrentCustomerAsync(customer);
                        paymentRequest = new ProcessPaymentRequest();

                        paymentRequest.CustomerId = tamaraTransaction.CustomerId;
                        paymentRequest.OrderTotal = tamaraTransaction.Amount;
                        paymentRequest.StoreId = tamaraTransaction.StoreId;
                        paymentRequest.OrderGuidGeneratedOnUtc = tamaraTransaction.CreatedOnUtc;
                        paymentRequest.PaymentMethodSystemName = TamaraDefault.SystemName;
                        paymentRequest.OrderGuid = orderGuid;
                        //var orderGuidKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.TamaraOrderId");
                        paymentRequest.CustomValues.Add(tamaraTransaction.OrderGuidKeyRes, orderGuid);
                        //paymentRequest.CustomValues.Add(orderGuid.ToString(), tamaraTransaction.OrderId);

                    }
                    var placeOrder = await _orderProcessingService.PlaceOrderAsync(paymentRequest);
                    if (placeOrder is null)
                    {
                        throw new NopException($"Could not place the order {orderGuid}");
                    }
                    var order = placeOrder.PlacedOrder;

                    if (order is null)
                    {
                        throw new NopException($"Could not find an order {orderGuid}");
                    }

                    //var requestString = JsonConvert.SerializeObject(orderDetailResponse);
                    //await _orderService.InsertOrderNoteAsync(new Core.Domain.Orders.OrderNote()
                    //{
                    //    OrderId = order.Id,
                    //    Note = $"Webhook details: {System.Environment.NewLine}{requestString}",
                    //    DisplayToCustomer = false,
                    //    CreatedOnUtc = DateTime.UtcNow
                    //});

                    //authorization 
                    var (authorise, _) = await AuthorizeAsync(settings, tamaraOrderId);
                    if (authorise.Status == "authorised")
                    {
                        order.AuthorizationTransactionId = authorise.OrderId;
                        order.AuthorizationTransactionResult = authorise.Status;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderProcessingService.MarkAsAuthorizedAsync(order);

                        var (capture, _) = await CaptureAsync(settings, orderDetailResponse);
                        switch (capture?.Status?.ToLowerInvariant())
                        {
                            case "fully_captured":
                                order.CaptureTransactionId = capture.CaptureId;
                                order.CaptureTransactionResult = capture.Status;
                                await _orderService.UpdateOrderAsync(order);
                                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {

                    await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
                }


                return true;
            });
        }

        /// <summary>
        /// Get order detail by id
        /// </summary>
        /// <param name="settings">Plugin settings</param>
        /// <param name="webhookUrl">Webhook URL</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the webhook; error message if exists
        /// </returns>
        public async Task<(OrderDetailsResponse OrderDetailsResponse, string Error)> GetOrderDetailAsync(TamaraSettings settings, string tamaraOrderId)
        {
            return await HandleFunctionAsync(async () =>
            {
                //ensure that plugin is configured
                if (string.IsNullOrEmpty(tamaraOrderId))
                    throw new NopException("Plugin not configured");

                var request = new OrderDetailsRequest(tamaraOrderId);
                var response = await _tamaraHttpClient.RequestAsync<OrderDetailsRequest, OrderDetailsResponse>(request, settings)
                        ?? throw new NopException($"Empty result");
                return response;
            });
        }

        #region TamaraTransaction

        public async Task CreateTamaraTransaction(string billingAddressEmail, decimal amount, string customerEmail,
                                            string customerName, string currencyCode, int customerId,
                                            bool isFromMobile/*, int? orderId*/, Guid orderReference, DateTime createdOn, int storeId, string orderGuidKeyRes)
        {
            try
            {
                TamaraPaymentTransaction tamaraPaymentTransaction = new TamaraPaymentTransaction
                {
                    BillingAddressEmail = billingAddressEmail,
                    Amount = amount,
                    CustomerEmail = customerEmail,
                    CustomerName = customerName,
                    CurrencyCode = currencyCode,
                    CustomerId = customerId,
                    IsMobile = isFromMobile,
                    //OrderId = orderId,
                    OrderReference = orderReference,
                    StoreId = storeId,
                    CreatedOnUtc = createdOn,
                    OrderGuidKeyRes = orderGuidKeyRes
                };

                await _tamaraPaymentTransactionRepository.InsertAsync(tamaraPaymentTransaction);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
        }
        public async Task<TamaraPaymentTransaction> GetTamaraTransactionByOrderReference(Guid orderReference)
        {
            var transactions = await _tamaraPaymentTransactionRepository
                .GetAllAsync(query => query.Where(x => x.OrderReference == orderReference));

            if (transactions != null && transactions.Any())
            {
                return transactions.Last();
            }

            return null;
        }
        #endregion
        #endregion

        #region Utilities
        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="function">Function</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result; error message if exists
        /// </returns>
        private async Task<(TResult Result, string Error)> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                //invoke function
                return (await function(), default);
            }
            catch (Exception exception)
            {
                //get a short error message
                var message = exception.Message;
                if (exception is HttpException httpException)
                {
                    //get error details if exist
                    var details = JsonConvert.DeserializeObject<ExceptionDetails>(httpException.Message);
                    message = details.Message?.Trim();
                }

                //log errors
                var logMessage = $"{TamaraDefault.SystemName} error: {System.Environment.NewLine}{message}";
                await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());

                return (default, message);
            }
        }
        #endregion
    }
}