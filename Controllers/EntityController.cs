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

        // Ёто общий ендпоинт дл€ получени€/добавлени€ Entity в зависимости от Query параметров. –азделить его на 2 ендпоинта
        // в соответствии с т.з. - не получитс€ т.к. в примерах на получение и добавление Entity роутинг совпадает.
        // –азделение на GET и POST - тоже не лучша€ иде€, т.к. передавать Query параметры в POST - плоха€ практика.
        // Ёто €вно не лучша€ практика, но это будет работать в соответствии с т.з.
        //  роме того, примеры запросов из т.з. не были перекодированы в URL и соответственно не будут валидными дл€ сериализации.
        // » хот€ данный метод будет работать (если перекодировать URL запрос из примера), лучше воспользоватьс€ REST методами
        // GetEntity и InsertEntity которые € создал в качестве альтернативного решени€. “акже, только дл€ REST методов будут Unit тесты.

        [Route("")]
        public ActionResult<Entity> EntityAction([FromQuery(Name = "get")] Guid? queryId = null, [FromQuery(Name = "insert")] string entityStr = "")
        {
            Guid id = default;
            try
            {
                id = queryId ?? Guid.Empty;
                // «апрет одновременного добавлени€ и получени€ Entity
                if (string.IsNullOrEmpty(entityStr) is false && id != Guid.Empty)
                    return StatusCode(400);

                // ƒобавление Entity
                if (string.IsNullOrEmpty(entityStr) is false)
                {
                    Entity newEntity = JsonConvert.DeserializeObject<Entity>(entityStr);
                    // ƒобавление Entity с пустым Guid - запрещено
                    if (newEntity.Id == Guid.Empty)
                        return StatusCode(400);

                    _entityStorage.InsertEntity(newEntity);
                    return Ok();
                }

                // ѕолучение Entity
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
                _logger.LogWarning($"Ќе удалось получить {nameof(Entity)} с {nameof(Entity.Id)}: {id} из хранилища. {ex.Message}");

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
                _logger.LogWarning($"Ќе удалось получить {nameof(Entity)} с {nameof(Entity.Id)}: {id} из хранилища. {ex.Message}");

                return StatusCode(400);
            }
        }

        // REST POST
        [HttpPost(ROUTE)]
        public ActionResult InsertEntity([FromBody] Entity entity)
        {
            try
            {
                // ƒобавление Entity с пустым Guid - запрещено
                if (entity.Id == Guid.Empty)
                    return StatusCode(400);

                _entityStorage.InsertEntity(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                if (entity is not null)
                    _logger.LogWarning($"Ќе удалось добавить новый {nameof(Entity)} с {nameof(Entity.Id)}: {entity.Id} в хранилище. {ex.Message}");
                else
                    _logger.LogWarning($"Ќе удалось добавить новый {nameof(Entity)} в хранилище. {ex.Message}");

                return StatusCode(400);
            }
        }
    }
}