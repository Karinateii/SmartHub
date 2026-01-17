namespace SmartHub.Domain.Common
{
    public abstract class BaseEntity
    {
        //Gives every entity a unique primary key
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}