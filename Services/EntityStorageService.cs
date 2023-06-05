using UnistreamTest.Services.Interfaces;

namespace UnistreamTest.Services
{
    public class EntityStorageService : IEntityStorage
    {
        private Dictionary<Guid, Entity> storage;

        public EntityStorageService()
        {
            storage = new Dictionary<Guid, Entity>();
        }


        public Entity GetById(Guid id)
        {
            Entity result = null;

            storage.TryGetValue(id, out result);

            return result;
        }

        public void InsertEntity(Entity entity)
        {
            storage[entity.Id] = entity;
        }
    }
}
