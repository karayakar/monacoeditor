using Microsoft.AspNetCore.Mvc;
using monacoEditorCSharp.Models;
 
using System;

namespace monacoEditorCSharp.Controllers
{
    [Route("api/compiler")]
    public class CompilerController : Controller
    {
        [HttpGet("get")]
        public dynamic Get()
        {

            var source = new SourceInfo();
            return source;

        }

        [HttpPost("resolve")]
        public dynamic Resolve([FromBody]SourceInfo source)
        {

            try
            {

                return CSharpScriptCompiler.CompileRos(source);

            }
            catch (Exception ex)
            {
                return ex.Message;
                //throw;
            }
        }

        [HttpPost("compile")]
        public dynamic Compile([FromBody]SourceInfo source)
        {

            try
            {

                return CSharpScriptCompiler.Compile(source);

            }
            catch (Exception ex)
            {
                return ex.Message;
                //throw;
            }
        }

        [HttpPost("formatcode")]
        public dynamic formatcode([FromBody]SourceInfo source)
        {

            try
            {

                return CSharpScriptCompiler.FormatCode(source);

            }
            catch (Exception ex)
            {
                return ex.Message;
                //throw;
            }
        }


    }
}
