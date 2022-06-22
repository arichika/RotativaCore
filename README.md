Extremely easy way to create Pdf files from ASP.NET Core
=========================================================

New Features
------

### 4.1.0

* Property `SaveOnServerPath` is discontinued.
* Support new delegate. `OnNoContentAsync()`
  * Specifies an alternative process in the case of an empty content response 
    from the conversion source or an error response (error response to standard output) 
    by wkhtmltopdf in converting an empty content.  
  * Within this delegate, direct operations (e.g., redirection instructions) must be performed on the ActionContext.
  * When this process is called, `TryCustomizeAsync` and `OnBuildFileSuccess` are not executed.
  * To use this feature effectively, it is essential to return `wkhtmltoX` with NoContent, i.e., an empty rendering element (empty Body response). This is because `wkhtmltoX` will respond with an error condition on its standard output and exit. 
  * To take advantage of this, I modified the internal processing so that `wkhtnmltoX` returns `WkhtmlDriverStandardErrorException` if `wkhtnmltoX` terminates abnormally and the return data is empty. If `OnNoContentAsync()` is defined in this case, this process is called because the converted data is always empty when this error is detected.
  * If `OnNoContentAsync()` is not defined, there is no difference in behavior except that the thrown Exception is more detailed.

```csharp
        public async Task<IActionResult> TestEmptyContentActionAsPdfInline()
        {
            return new ActionAsPdf("EmptyContent")
            {
                ContentDisposition = ContentDisposition.Inline,
                OnNoContentAsync = async (ex, context) =>
                {
                    // e.g.
                    context.HttpContext.Response.Redirect("https://example.com/");
                }
            };
        }
```


### 4.0.0

* Property `SaveOnServerPath` is discontinued.
* Support new delegate. `TryCustomizeAsync()`
  * If you want to customize the generated binary file before `OnBuildFileSuccess()`, use this.
  * Please return true to continue processing, false to drop with error.

```csharp
        public ActionResult TestInlie()
        {
            return new ActionAsPdf("Index")
            {
                ContentDisposition = ContentDisposition.Inline,
                TryCustomizeAsync = async (stream, context, fineName) =>
                {
                    // some code done.
                    return true;

                    // e.g.
                    var customizeStream = new MemoryStream();
                    await stream.CopyToAsync(customizeStream);

                    // ...
                    stream.SetLength(0);
                    await customizeStream.CopyToAsync(stream);

                    return true;
                },
            };
        }
```


### 3.0.0

* Support ASP.NET Core 3.0.  Thank you [@vertonghenb](https://github.com/vertonghenb)

### 2.2.0

* Support new event. `OnBuildFileSuccess()`

```csharp
        public ActionResult TestInlie()
        {
            return new ActionAsPdf("Index", new { name = "Friends" })
            {
                //FileName = "Test.pdf",
                ContentDisposition = ContentDisposition.Inline,
                OnBuildFileSuccess = async (bytes, context, fileName) =>
                {
                    // some code done.
                    return true;

                    // example.
                    if (string.IsNullOrEmpty(fileName))
                        fileName = $"{Guid.NewGuid()}.pdf";

                    var container = CloudStorageAccount
                         // Please set your value.
                         // If it's null, it will result in an ArgumentNullException().
                        .Parse(connectionString:null)
                        .CreateCloudBlobClient()
                        // Please set your value.
                        // If it's null, it will result in an ArgumentNullException().
                        .GetContainerReference(containerName:null);

                    try
                    {
                        var blockBlob = container.GetBlockBlobReference(fileName);
                        blockBlob.Properties.ContentType = "application/pdf";
                        await blockBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                    }
                    catch (Exception e)
                    {
                        // logging.
                        return false;  // fire InvalidOperationException()
                    }

                    return true;
                },
            };
        }
```

Usage
------

```csharp
public ActionResult PrintIndex()
{
    return new ActionAsPdf("Index", new { name = "Giorgio" }) { FileName = "Test.pdf" };
}

public ActionResult Index(string name)
{
    ViewBag.Message = string.Format("Hello {0} to ASP.NET MVC!", name);

    return View();
}
```

ViewAsPdf now available. It enables you to render a view as pdf in just one move, thanks to scoorf

```csharp
public ActionResult TestViewWithModel(string id)
{
    var model = new TestViewModel {DocTitle = id, DocContent = "This is a test"};
    return new ViewAsPdf(model);
}
```

Also available a RouteAsPdf, UrlAsPdf and ViewAsPdf ActionResult.

It generates Pdf also from authorized actions (web forms authentication).

You can also output images from MVC with ActionAsImage, ViewAsImage, RouteAsImage, UrlAsImage.
