namespace UnistreamTest.Services.Interfaces
{
    public interface IEntityStorage
    {
        public void InsertEntity(Entity entity);
        public Entity GetById(Guid id);
    }
}
