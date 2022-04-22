Extremely easy way to create Pdf files from ASP.NET Core
=========================================================

New Features
------

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
