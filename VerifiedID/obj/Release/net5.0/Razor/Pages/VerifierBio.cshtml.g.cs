#pragma checksum "C:\Users\elias.ekonomou\OneDrive - Avanade\MS Identity Hackathon\Issuer\active-directory-verifiable-credentials-dotnet\1-asp-net-core-api-idtokenhint\Pages\VerifierBio.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e8a71a4d30c7f0ab847d3e381c6b003ac9add233"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCoreVerifiableCredentials.Pages.Pages_VerifierBio), @"mvc.1.0.razor-page", @"/Pages/VerifierBio.cshtml")]
namespace AspNetCoreVerifiableCredentials.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\elias.ekonomou\OneDrive - Avanade\MS Identity Hackathon\Issuer\active-directory-verifiable-credentials-dotnet\1-asp-net-core-api-idtokenhint\Pages\_ViewImports.cshtml"
using AspNetCoreVerifiableCredentials;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e8a71a4d30c7f0ab847d3e381c6b003ac9add233", @"/Pages/VerifierBio.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c9a6c6105e21e45cb7ef575038cfac3da5f8e208", @"/Pages/_ViewImports.cshtml")]
    public class Pages_VerifierBio : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n<div style=\"text-align: center;\">\r\n    <h1>Orange Interstellar Corp Events</h1>\r\n    <div id=\"text\"");
            BeginWriteAttribute("style", " style=\"", 178, "\"", 186, 0);
            EndWriteAttribute();
            WriteLiteral(@">
        <p class=""small-text"">
            Click Capture to take a selfie and click Next when you are happy with it. Or click Capture to again to yake a new selfie.
        </p>
    </div>
    <div id=""qrcode"" style=""text-align: center"">

        <video id=""myVidPlayer"" controls muted autoplay hidden></video>

        <button class=""button"" onclick=""snapshot()"">Capture</button>
        <button class=""button"" onclick=""base64()"">Next</button>

        <div class=""test"" id=""test1"">
            <div class=""mycanvas"">
                <canvas></canvas>
            </div>
        </div>

    </div>

    <script>
        var canvas = document.querySelector(""canvas"");
        var context = canvas.getContext(""2d"");
        const video = document.querySelector('#myVidPlayer');
        var id = """";
        var index = location.search.indexOf(""="");
        var id = location.search.substr(++index, location.search.length - 1);

        //w-width,h-height
        var w, h;
        canvas.style");
            WriteLiteral(@".display = ""none"";


        function base64() {

            //const dataURL = canvas.toDataURL();
            //console.log(dataURL);

            canvas.toBlob(function (blob) {
                const formData = new FormData();
                formData.append(""image"", blob, ""file.png"");
                formData.append(""id"", id);
                const request = new XMLHttpRequest();
                request.open(""POST"", ""/api/verifier/submit-image?id="" + id);
                request.send(formData);
                request.onreadystatechange = function () {
                    if (request.readyState == XMLHttpRequest.DONE) {
                        response = JSON.parse(request.responseText);
                        window.location.href = ""Verifier?id="" + response.id;
                    }
                }
            });

        }


        //new
        function snapshot() {
            context.fillRect(0, 0, w, h);
            context.drawImage(video, 0, 0, w, h);
           ");
            WriteLiteral(@" canvas.style.display = ""block"";
        }



        window.navigator.mediaDevices.getUserMedia({ video: true, audio: false })
            .then(stream => {
                video.srcObject = stream;
                video.onloadedmetadata = (e) => {
                    video.play();


                    w = 448;
                    h = 336;

                    canvas.width = w;
                    canvas.height = h;
                };
            })
            .catch(error => {
                alert('Please allow the camera');
            });
    </script>

</div>

");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<AspNetCoreVerifiableCredentials.Pages.VerifierBioModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<AspNetCoreVerifiableCredentials.Pages.VerifierBioModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<AspNetCoreVerifiableCredentials.Pages.VerifierBioModel>)PageContext?.ViewData;
        public AspNetCoreVerifiableCredentials.Pages.VerifierBioModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591