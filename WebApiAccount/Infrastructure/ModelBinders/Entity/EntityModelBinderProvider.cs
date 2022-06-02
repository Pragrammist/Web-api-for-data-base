using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountsDataBaseWebApi;


namespace WebApiAccount.Infrastructure.ModelBinders
{
    public class EntityModelBinderProvider<T> : IModelBinderProvider where T: Entity
    {
        public EntityModelBinderProvider(IModelBinder binder) => _binder = binder;
        public EntityModelBinderProvider() { }

        protected readonly IModelBinder _binder = new EntityModelBinder<T>();
        public virtual IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            // Для объекта SimpleTypeModelBinder необходим сервис ILoggerFactory
            // Получаем его из сервисов
            return context.Metadata.ModelType == typeof(T) ? _binder : null;
        }
    }
    public class UserModelBinderProvider : EntityModelBinderProvider<User>
    {
        public UserModelBinderProvider() : base(new UserModelBinder())
        {
            
        }
    }
    public class AvatarModelBinderProvider : EntityModelBinderProvider<Avatar>
    {

    }
    public class BillModelBinderProvider : EntityModelBinderProvider<Bill>
    {

    }
    public class GuildModelBinderProvider : EntityModelBinderProvider<Guild>
    {

    }
    public class OrderModelBinderProvider : EntityModelBinderProvider<Order>
    {

    }
    public class ProductModelBinderProvider : EntityModelBinderProvider<Product>
    {

    }
    public class ReportModelBinderProvider : EntityModelBinderProvider<Report>
    {

    }
}
