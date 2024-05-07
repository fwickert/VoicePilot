using Microsoft.AspNetCore.Mvc;
using VoicePilot.Services;

namespace VoicePilot_Web.ViewComponents
{
    public class ActionRecoComponent : ViewComponent
    {
        private readonly CustomResponse _actionRecoResponse;

        public ActionRecoComponent([FromServices] CustomResponse actionRecoResponse)
        {
            _actionRecoResponse = actionRecoResponse;
            _actionRecoResponse.PluginName = "VoiceBot";
            _actionRecoResponse.FunctionName = "ActionReco";
        }

        public async Task<IViewComponentResult> InvokeAsync(string input)
        {
            var result = await _actionRecoResponse.GetAsync("ActionReco",
                new Dictionary<string, string>()
                {
                    { "input", input}
                });
            return View(result);
        }
    }
}
