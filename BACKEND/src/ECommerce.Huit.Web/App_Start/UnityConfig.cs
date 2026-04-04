using System.Web.Mvc;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.Services;
using ECommerce.Huit.Infrastructure.Data;
using Microsoft.Practices.Unity;
using Unity.Mvc5;

namespace ECommerce.Huit.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register all your components with the container here
            // it is NOT necessary to register your controllers

            // Context
            container.RegisterType<IApplicationDbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());

            // Services
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<IProductService, ProductService>();
            container.RegisterType<ICartService, CartService>();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IJwtTokenGenerator, JwtTokenGenerator>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
