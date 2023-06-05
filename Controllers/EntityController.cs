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

        // ��� ����� �������� ��� ���������/���������� Entity � ����������� �� Query ����������. ��������� ��� �� 2 ���������
        // � ������������ � �.�. - �� ��������� �.�. � �������� �� ��������� � ���������� Entity ������� ���������.
        // ���������� �� GET � POST - ���� �� ������ ����, �.�. ���������� Query ��������� � POST - ������ ��������.
        // ��� ���� �� ������ ��������, �� ��� ����� �������� � ������������ � �.�.
        // ����� ����, ������� �������� �� �.�. �� ���� �������������� � URL � �������������� �� ����� ��������� ��� ������������.
        // � ���� ������ ����� ����� �������� (���� �������������� URL ������ �� �������), ����� ��������������� REST ��������
        // GetEntity � InsertEntity ������� � ������ � �������� ��������������� �������. �����, ������ ��� REST ������� ����� Unit �����.

        [Route("")]
        public ActionResult<Entity> EntityAction([FromQuery(Name = "get")] Guid? queryId = null, [FromQuery(Name = "insert")] string entityStr = "")
        {
            Guid id = default;
            try
            {
                id = queryId ?? Guid.Empty;
                // ������ �������������� ���������� � ��������� Entity
                if (string.IsNullOrEmpty(entityStr) is false && id != Guid.Empty)
                    return StatusCode(400);

                // ���������� Entity
                if (string.IsNullOrEmpty(entityStr) is false)
                {
                    Entity newEntity = JsonConvert.DeserializeObject<Entity>(entityStr);
                    // ���������� Entity � ������ Guid - ���������
                    if (newEntity.Id == Guid.Empty)
                        return StatusCode(400);

                    _entityStorage.InsertEntity(newEntity);
                    return Ok();
                }

                // ��������� Entity
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
                _logger.LogWarning($"�� ������� �������� {nameof(Entity)} � {nameof(Entity.Id)}: {id} �� ���������. {ex.Message}");

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
                _logger.LogWarning($"�� ������� �������� {nameof(Entity)} � {nameof(Entity.Id)}: {id} �� ���������. {ex.Message}");

                return StatusCode(400);
            }
        }

        // REST POST
        [HttpPost(ROUTE)]
        public ActionResult InsertEntity([FromBody] Entity entity)
        {
            try
            {
                // ���������� Entity � ������ Guid - ���������
                if (entity.Id == Guid.Empty)
                    return StatusCode(400);

                _entityStorage.InsertEntity(entity);
                return Ok();
            }
            catch (Exception ex)
            {
                if (entity is not null)
                    _logger.LogWarning($"�� ������� �������� ����� {nameof(Entity)} � {nameof(Entity.Id)}: {entity.Id} � ���������. {ex.Message}");
                else
                    _logger.LogWarning($"�� ������� �������� ����� {nameof(Entity)} � ���������. {ex.Message}");

                return StatusCode(400);
            }
        }
    }
}