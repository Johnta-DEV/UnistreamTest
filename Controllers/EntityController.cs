using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnistreamTest.Services.Interfaces;

namespace UnistreamTest.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [Route("")]
    public class EntityController : ControllerBase
    {
        private const string ROUTE = "[controller]";

        private readonly ILogger<EntityController> _logger;
        private readonly IEntityStorage _entityStorage;

        public EntityController(ILogger<EntityController> logger, IEntityStorage entityStorage)
        {
            _logger = logger;
            _entityStorage = entityStorage;
        }

        // Это общий ендпоинт для получения/добавления Entity в зависимости от Query параметров. Разделить его на 2 ендпоинта
        // в соответствии с т.з. - не получится т.к. в примерах на получение и добавление Entity роутинг совпадает.
        // Разделение на GET и POST - тоже не лучшая идея, т.к. передавать Query параметры в POST - плохая практика.
        // Это явно не лучшая практика, но это будет работать в соответствии с т.з.
        // Кроме того, примеры запросов из т.з. не были перекодированы в URL и соответственно не будут валидными для сериализации.
        // И хотя данный метод будет работать (если перекодировать URL запрос из примера), лучше воспользоваться REST методами
        // GetEntity и InsertEntity которые я создал в качестве альтернативного решения. Также, только для REST методов будут Unit тесты.

        [Route("")]
        public ActionResult<Entity> EntityAction([FromQuery(Name = "get")] Guid? queryId = null, [FromQuery(Name = "insert")] string entityStr = "")
        {
            Guid id = default;
            try
            {
                id = queryId ?? Guid.Empty;
                // Запрет одновременного добавления и получения Entity
                if (string.IsNullOrEmpty(entityStr) is false && id != Guid.Empty)
                    return StatusCode(400);

                // Добавление Entity
                if (string.IsNullOrEmpty(entityStr) is false)
                {
                    Entity newEntity = JsonConvert.DeserializeObject<Entity>(entityStr);
                    // Добавление Entity с пустым Guid - запрещено
                    if (newEntity.Id == Guid.Empty)
                        return StatusCode(400);

                    _entityStorage.InsertEntity(newEntity);
                    return Ok();
                }

                // Получение Entity
                if (id != Guid.Empty)
                {
                    Entity entityFromStorage = _entityStorage.GetById(id);

                    if (entityFromStorage is null)
                        return StatusCode(404);

                    var result = JsonConvert.SerializeObject(entityFromStorage);

                    return Ok(result);
                }

                return StatusCode(400);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Не удалось получить {nameof(Entity)} с {nameof(Entity.Id)}: {id} из хранилища. {ex.Message}");

                return StatusCode(400);
            }
        }



        // REST GET
        [HttpGet(ROUTE + "/{id}")]
        public ActionResult GetEntity(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return StatusCode(400);

                Entity entity = _entityStorage.GetById(id);

                if (entity is null)
                    return StatusCode(404);

                var result = JsonConvert.SerializeObject(entity);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Не удалось получить {nameof(Entity)} с {nameof(Entity.Id)}: {id} из хранилища. {ex.Message}");

                return StatusCode(400);
            }
        }

        // REST POST
        [HttpPost(ROUTE)]
        public ActionResult InsertEntity([FromBody] Entity entity)
        {
            try
            {
                // Добавление Entity с пустым Guid - запрещено
                if (entity.Id == Guid.Empty)
                    return StatusCode(400);

                _entityStorage.InsertEntity(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                if (entity is not null)
                    _logger.LogWarning($"Не удалось добавить новый {nameof(Entity)} с {nameof(Entity.Id)}: {entity.Id} в хранилище. {ex.Message}");
                else
                    _logger.LogWarning($"Не удалось добавить новый {nameof(Entity)} в хранилище. {ex.Message}");

                return StatusCode(400);
            }
        }
    }
}