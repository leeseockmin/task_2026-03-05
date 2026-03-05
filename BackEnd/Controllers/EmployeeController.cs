using BackEnd.Application.Commands.Employee;
using BackEnd.Application.DTOs.Common;
using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Queries.Employee;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IMediator mediator, ILogger<EmployeeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>이름으로 직원 상세 연락정보를 조회합니다.</summary>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeByNameAsync(string name)
        {
            var result = await _mediator.Send(new GetEmployeeByNameQuery(name));

            if (result is null)
            {
                return NotFound(new ErrorResponse($"이름 '{name}'에 해당하는 직원이 없습니다."));
            }

            return Ok(result);
        }

        /// <summary>직원 목록을 페이지 단위로 조회합니다.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(EmployeeListResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEmployeeListAsync([FromQuery] GetEmployeeListRequest request)
        {
            if (request.Page < 1 || request.PageSize < 1)
            {
                _logger.LogWarning(
                    "잘못된 페이지 파라미터. Page: {Page}, PageSize: {PageSize}",
                    request.Page, request.PageSize);

                return BadRequest(new ErrorResponse("page와 pageSize는 1 이상이어야 합니다."));
            }

            var result = await _mediator.Send(new GetEmployeeListQuery(request.Page, request.PageSize));
            return Ok(result);
        }

        /// <summary>직원을 등록합니다.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEmployeeAsync([FromBody] CreateEmployeeRequest request)
        {
            await _mediator.Send(
                new CreateEmployeeCommand(request.Name, request.Email, request.Tel, request.Joined));

            return StatusCode(StatusCodes.Status201Created);
        }
    }
}
