# EpiCommonUtils

This is a set of common stuff which might be helpfull with your day to day EpiServer work

## Installation

It's available as a [nuget package](https://www.nuget.org/packages/Forte.EpiCommonUtils/)
 
## Key features

### Validation attributes
There are couple of additional validation attributes for Episerver model classes available:

#### MaxItemsAllowed

This attribute can be placed on property/field of type `ContentArea`, `LinkItemCollection` or any `IEnumerable` to limit the maximum number of items linked in them

#### AllowedItemsCount
This attribute, placed on property/field of type `ContentArea`, `LinkItemCollection` or any `IEnumerable`, will constraint amount of items to given range (inclusive).

#### XHtmlStringAllowedTypes

Can be placed on property/field of `XHtmlString` type. It limits types of content that can be placed in the content (via TinyMCE), similar to Episerver-builtin `AllowedTypesAttribute`. Keep in mind that it works for both block and media types.

### Editor descriptors

Additional defined editor descriptors:

#### EnumEditorDescriptor
Allow to use enums in Episerver models, selectable by editors. Usage:

```cs
public enum EnumType {
    Value1 = 1,
    Value2 = 2,
    [EpiUnselectable] IgnoredValue = 3 // add explicit values to make it less fragile for refactoring
}

public class SomePage: PageData {
    [BackingType(typeof(PropertyNumber))]
    [EditorDescriptor(EditorDescriptorType = typeof(EnumEditorDescriptor<EnumType>))]
}
```
In order to translate enum values for editor, define following in language XML files:
```xml
<languages>
    <language>
        <enum>
            <EnumType>
                <Value1>Value 1</Value1>
                <Value2>Value 1</Value2>
                <IgnoredValue>This value is irrelevant, as won't be shown to editors</IgnoredValue>
    </language>
</languages>
```


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
With versions 2.0+, image HTML helpers for responsive pictures and backgrounds were moved to separate NuGet package: [Forte.EpiResponsivePicture](https://github.com/fortedigital/EpiResponsivePicture).

### TranslationFileGenerator

`TranslationFileGenerator` generates strongly typed class out of .xml language files. Invoke static `Generate` method in `TranslationGenerationInitializer` for easy use, passing source and target directory as arguments or run `Generate` method directly on generator providing source directory, root namespace and class name.

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
