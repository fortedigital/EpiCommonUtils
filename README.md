# EpiCommonUtils

This is a set of common stuff which might be helpfull with your day to day EpiServer work

## Installation

It's available as a [nuget package](https://www.nuget.org/packages/Forte.EpiCommonUtils/)
 
## Key features
### HostingEnvironment

Offers several flags saying on which environment solution is working on. It's based on `episerver:EnvironmentName` appSetting:

        IsLocalDev
        IsIntegration
        IsDev
        IsPreProd
        IsProd 


### Webpack

It offers two methods. 

`public static string GetStylesheetUrl(string name)`


`public static string GetScriptUrl(string name)`

Those return paths to script/stylesheet containing unique hash generated by webpack. If solution is running on `IsLocalDev` it returns path to local webpack dev server.

You can configure path to local webpack dev server by setting `webpack:devServerUrl` appSetting value (the default is `https://localhost:8080/dist/`)

In order this tool work properly it requires file generated by [Assets webpack plugin](https://www.npmjs.com/package/assets-webpack-plugin). Default path checked is set to `/dist/webpack-assets.json`. 
You can override it by setting `webpack:manifestPath` appSetting 

### FeatureViewLocationRazorEngine

This is extension of `RazonViewEngine` and it sets up location of templates with common pattern we've been using. 
Check source code of `FeatureViewLocationRazorEngine` for path details. 

This is by default initialized by EpiServer startup. You can disable this initialization by setting up `appSetting` called `epiCommonUtils:disableFeatureViewLocationRazor` with value `true`

### StructureMap - DependencyInjection

In order for `StructureMap` to work as your Dependency Injection container you must register it - separately for MVC and WebApi controllers.
This package will do it for you by default. 

If you wish to disable this registration you can do it by setting flags in `appSettings`:

```xml

<appSettings>
    <!-- Disable MVC registration -->
    <add key="epiCommonUtils:disableStructureMapMvcRegistration" value="true" />
    <!-- Disable WebApi registration -->
    <add key="epiCommonUtils:disableStructureMapWebApiRegistration" value="true" />
</appSettings>
``` 

### Image html helpers

There are two html helpers method registered. Both of them make use of `PictureProfile` class which is also a part of this package:

```c#
public class PictureProfile
{
    public int DefaultWidth { get; set; }
    public int[] SrcSetWidths { get; set; }
    public string[] SrcSetSizes { get; set; }
    public int? MaxHeight { get; set; }
    public ScaleMode Mode { get; set; }
    public int? Quality { get; set; }
}
```

#### ResizedImageUrl

This will generate url to resized image. Example usage (`Model` is supposed to be `ContentReference` instance):
```razor
<img src="@Html.ResizedImageUrl(Model, 2048, 1000, new PictureProfile{Mode = ScaleMode.Crop})" />
```

#### ResizedPicture

_NOTE: In order this method to load `alt` element properly Image property should be of type `Forte.EpiCommonUtils.Infrastructure.Model.ImageBase`_

This will generate `picture` element with responsive support. So, having defined this (there's no predefined profiles in this package) :

```c#
public static class PictureProfiles
{
    public static readonly PictureProfile Hero = new PictureProfile
    {
        DefaultWidth = 1500,
        SrcSetWidths = new[] { 400, 800, 1200, 1600 },
        SrcSetSizes = new[]
        {
            "(min-width: 1200px) 1400px",
            "(min-width: 1140px) 1140px",
            "(max-width: 1139px) 100vw",
        },           
        Mode = ScaleMode.Crop,
        Quality = 60
    };
}
``` 

by doing this:

```razor
@Html.ResizedPicture(Model, PictureProfiles.Hero)
```

you will get this piece of markup:
```html
<picture>
    <source sizes="(min-width: 1200px) 1400px, (min-width: 1140px) 1140px, (max-width: 1139px) 100vw" srcset="/contentassets/92a71a8e82a94be3ab5581d099a68f48/1.jpg?w=400&mode=crop&quality=60 400w, /contentassets/92a71a8e82a94be3ab5581d099a68f48/1.jpg?w=800&mode=crop&quality=60 800w, /contentassets/92a71a8e82a94be3ab5581d099a68f48/1.jpg?w=1200&mode=crop&quality=60 1200w, /contentassets/92a71a8e82a94be3ab5581d099a68f48/1.jpg?w=1600&mode=crop&quality=60 1600w" />
    <img alt="alternate custom" data-object-fit="cover" data-object-position="center" src="/contentassets/92a71a8e82a94be3ab5581d099a68f48/1.jpg?w=1500&mode=crop&quality=60" />
</picture>

```

#### Responsive background
There is also similar way to create responsive, screen size based backgrounds. As background is defined by CSS, there is `ResizeBackground` HTML helper method available:
```razor 
@{
    var backgroundClass = @Html.ResizeBackground(Model, PictureProfiles.DefaultBackground);
}

<div class="@backgroundClass"></div>
```

Method will render piece of CSS code with proper media queries and returns name of CSS class that should be applied to the element in order to add background to it. It accepts slightly different, background picture profile: 
```cs
public static class PictureProfiles
{
    public static readonly BackgroundPictureProfile DefaultBackground = new BackgroundPictureProfile
    {
        AllowedSizes = new[]
        {
            new PictureSize("min-width: 1200px", 1400),
            new PictureSize("min-width: 800px", 1200),
            new PictureSize("min-width: 600px", 800),
            new PictureSize("max-width: 599px", 600),
        },
    };
}

```

### TransformedXhtmlString and IHtmlTransformation

A `@Html.TransformedXhtmlString` helper method is registered. You can pass `XhtmlString` instance together with two optional class names which will be used as css classes to wrap xmtml and block.

Example:

Let's imagine this piece of TinyMCE (XhtmlString) content:

```
First paragraph
Second paragraph
[Inline block embedded into XhtmlString field]
Third paragraph
```

By rendering `@Html.TransformedXhtmlString(XhtmlStringValue, "textWrapperClass", "blockWrappedClass")` you will get this piece of html:

```html
<div class="textWrapperClass">
    <p>First paragraph</p>
    <p>Second paragraph</p>
</div>
<section class="blockWrappedClass">
    <!-- block rendered -->
</section>
<div class="textWrapperClass">
    <p>Third paragraph</p>
</div>
```

Optionally, you can pass list of types of blocks which should be not wrapped by container. 
Let's assume  `[Inline block embedded into XhtmlString field]` is an instance of `DummyBlockType`. 
By doing this:
`@Html.TransformedXhtmlString(XhtmlStringValue, "textWrapperClass", "blockWrappedClass", new [] {typeof(DummyBlockType)})`

you will get this html rendered:
```html
<div class="textWrapperClass">
    <p>First paragraph</p>
    <p>Second paragraph</p>
</div>
<!-- block rendered -->
<div class="textWrapperClass">
    <p>Third paragraph</p>
</div>
```

Additionaly, you can create class which implements `IHtmlTransformation` and register it within `ServiceLocator` as `IHtmlTransformation` implementation. 
This way you can define additional transformation which will be applied to your `XhtmlString` content.

#### HtmlTransformationAttribute

There's `HtmlTransformationAttribute` defined. It has `Order` field which might be used to manipulate order of transformation applied (lower number means higher precedence).
Use this attribute to annotate your class implementing `IHtmlTransformation`.

NOTE: you **don't** need `HtmlTransformation` attribute in order transformation to be applied. It's only used for order manipulation purposes. 


