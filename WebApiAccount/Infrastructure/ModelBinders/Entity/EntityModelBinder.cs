using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using UserAccountsDataBaseWebApi;
using System.Text;



namespace WebApiAccount.Infrastructure.ModelBinders
{
    public class EntityModelBinder<T> : IModelBinder where T: Entity
    {
        public EntityModelBinder()
        {
            
        }
        public virtual async Task BindModelAsync(ModelBindingContext bindingContext)
        {

            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            T entity = null;
            
            var isFaulted = false;
            try
            {
                var streamContext = bindingContext.HttpContext.Request.BodyReader.AsStream();
                var buffer = new byte[1024];
                List<byte> addBuffer = new();
                int readed;

                do
                {
                    readed = await streamContext.ReadAsync(buffer, 0, buffer.Length);
                    addBuffer.AddRange(buffer[..readed]);
                } while (readed != 0);

                var bytes = addBuffer.ToArray();
                var json = Encoding.UTF8.GetString(bytes);

                entity = JsonSerializer.Deserialize<T>(json);
                //var serealizer = JsonSerializer.sERE
                
            }
            catch(Exception ex) 
            {
                isFaulted = true;
            }
            if (isFaulted)
                bindingContext.Result = ModelBindingResult.Failed();
            else
                bindingContext.Result = ModelBindingResult.Success(entity);

            //return Task.CompletedTask;
        }
    }


    public class UserModelBinder : EntityModelBinder<User>
    {
        public override async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await base.BindModelAsync(bindingContext);
            if (bindingContext.Result != ModelBindingResult.Failed())
            {
                var user = (User)bindingContext.Result.Model;

                if (!string.IsNullOrWhiteSpace(user.StrPassword))
                    user.Password = User.GetHashCode(user.StrPassword);                
            }
            //return res;
        }
    }
}
