using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;


namespace Nop.Plugin.Payments.Tamara.Services;

/// <summary>
/// Represents the plugin service manager
/// </summary>
public class ServiceManager
{
    #region Fields

    protected readonly CurrencySettings _currencySettings;
    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly IAddressService _addresService;
    protected readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILogger _logger;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IOrderService _orderService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IProductService _productService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreService _storeService;
    protected readonly ITaxService _taxService;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public ServiceManager(CurrencySettings currencySettings,
        IActionContextAccessor actionContextAccessor,
        IAddressService addresService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
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
        IWorkContext workContext
       )
    {
        _currencySettings = currencySettings;
        _actionContextAccessor = actionContextAccessor;
        _addresService = addresService;
        _checkoutAttributeParser = checkoutAttributeParser;
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
    }

    #endregion


    #region Methods

    /// <summary>
    /// Check whether the plugin is configured
    /// </summary>
    /// <param name="settings">Plugin settings</param>
    /// <returns>Result</returns>
    public static bool IsConfigured(TamaraSettings settings)
    {
        //client id and secret are required to request services
        return !string.IsNullOrEmpty(settings?.PublicKey) && !string.IsNullOrEmpty(settings?.SecretKey);
    }

    #endregion

}