using WebApiAccount.Services;

namespace WebApiAccount.Models
{
    public class ControllerResult<EntityType, ResultType, ValidateEntityType>
    {
        public ControllerResult() { }
        public ControllerResult(EntityType entity, ResultType result, ValidationResult<ValidateEntityType> validation) 
        {
            Result = result;
            Entity = entity;
            ValidationResult = validation;
        }
        public ResultType Result { get; set; } // result of controller
        public EntityType Entity { get; set; } // main entity
        public ValidationResult<ValidateEntityType> ValidationResult { get; set; } // entity than validated
        
    }
}
