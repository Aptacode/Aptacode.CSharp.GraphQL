using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aptacode.CSharp.GraphQL.Models;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Aptacode.CSharp.GraphQL.Controllers
{
    /// <summary>
    ///     Generic MVC Controller that exposes a GraphQL endpoint
    /// </summary>
    public class GraphQLController : Controller
    {
        private readonly IDocumentExecuter _documentExecutor;
        private readonly ISchema _schema;

        public GraphQLController(ISchema schema, IDocumentExecuter documentExecutor)
        {
            _schema = schema ?? throw new NullReferenceException(nameof(ISchema));
            _documentExecutor = documentExecutor ?? throw new NullReferenceException(nameof(IDocumentExecuter));
        }

        /// <summary>
        ///     GraphQLQuery Http Action
        /// </summary>
        /// <param name="query"></param>
        /// <param name="validationRules"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query,
            [FromServices] IEnumerable<IValidationRule> validationRules)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var inputs = query.Variables.ToInputs();
            var executionOptions = new ExecutionOptions
            {
                Schema = _schema,
                Query = query.Query,
                Inputs = inputs,
                UserContext = new GraphQLUserContext
                {
                    User = User
                },
                ValidationRules = validationRules,
#if (DEBUG)
                ExposeExceptions = true,
                EnableMetrics = true,
#endif
            };

            var result = await _documentExecutor.ExecuteAsync(executionOptions).ConfigureAwait(false);

            if (result.Errors?.Count > 0)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}