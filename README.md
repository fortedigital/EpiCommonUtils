# EpiCommonUtils

This is a set of common stuff which might be helpful with your day to day EpiServer work

## Installation

It's available as a [nuget package](https://www.nuget.org/packages/Forte.EpiCommonUtils/)

Then, in your Startup, you need to register the package by running

```c#
services.AddEpiCommonUtils();
```

You can pass optional Action which will change default values for `EpiCommonUtilsOptions` 
## Key features

### Treating Index() as default action

In EPiServer 12, Index() is not treated as default action in controllers, which inherit from PageController<>. So when you create 2 methods in single controller and Index method is placed as second, then trying to render view without typing "/index" at the end would cause first action to invoke.
If you would like to turn off this behaviour, then during configuration of services you should register package by running

```c#
services.AddEpiCommonUtils(options => options.IndexAsDefaultAction = false);
```

### Validation attributes
There are a couple of additional validation attributes for Episerver model classes available:

#### MaxItemsAllowed

This attribute can be placed on a property/field of type `ContentArea`, `LinkItemCollection`, or any `IEnumerable` to limit the maximum number of items linked to them

#### AllowedItemsCount
This attribute, placed on property/field of type `ContentArea`, `LinkItemCollection` or any `IEnumerable`, will constrain a number of items to the given range (inclusive).

#### XHtmlStringAllowedTypes

Can be placed on property/field of `XHtmlString` type. It limits the types of content that can be placed in the content (via TinyMCE), similar to Episerver-builtin `AllowedTypesAttribute`. Keep in mind that it works for both block and media types.

#### XhtmlStringAllowedBlockTypes

Same as `[XhtmlStringAllowedTypes]`, but restricts only block types, not media.

#### ContentWithSameTemplate

Validates if content placed in Content Area uses the same template for rendering. Useful when modeling list blocks.

### Editor descriptors

Additionally defined editor descriptors:

#### EnumEditorDescriptor
Allow to use enums in Episerver models, selectable by editors. Usage:

```cs
public enum EnumType {
    [Display(Name = "Custom text")] // set value displayed to editor manually (if you don't want to use translations)
    Value1 = 1,
    Value2 = 2,
    [EpiUnselectable] IgnoredValue = 3 // add explicit values to make it less fragile for refactoring
}

public class SomePage: PageData {
    [BackingType(typeof(PropertyNumber))]
    [EditorDescriptor(EditorDescriptorType = typeof(EnumEditorDescriptor<EnumType>))]
    public virtual EnumType SomeEnum { get; set; }
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
            </EnumType>
         </enum>
    </language>
</languages>
```

#### NullableEnumEditorDescriptor

This is a variation of `EnumEditorDescriptor` with one difference: by using `NullableEnumEditorDescriptor` you will get an extra "empty" choice in the dropdown which will set a null value to your property

### Webpack

It offers two methods. 

`public static string GetStylesheetUrl(string name)`


`public static string GetScriptUrl(string name)`

Those return paths to script/stylesheet containing unique hash generated by webpack. If the solution is running on `Development` environment it returns the path to local webpack dev server.

You can configure the path to the local webpack dev server by setting `EpiCommonUtilsOptions.WebpackDevServerUrl` property (the default is `http://localhost:8080/dist/`)

In order for this tool to work properly, it requires a file generated by [Assets webpack plugin](https://www.npmjs.com/package/assets-webpack-plugin). The default path checked is set to `/dist/webpack-assets.json`. 
You can override it by setting `EpiCommonUtilsOptions.WebpackManifestPath` property 

### TranslationFileGenerator

`TranslationFileGenerator` generates strongly typed class out of .xml language files. Invoke static `Generate` method in `TranslationGenerationInitializer` for easy use, passing source, and target directory as arguments, or run `Generate` method directly on generator providing source directory, root namespace, and class name.

### TransformedXhtmlString and IHtmlTransformation

A `@Html.TransformedXhtmlString` helper method is registered. You can pass `XhtmlString` instance together with two optional class names which will be used as css classes to wrap xhtml and block.

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

Optionally, you can pass a list of types of blocks that should be not wrapped by the container. 
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
This way you can define additional transformations which will be applied to your `XhtmlString` content.

#### HtmlTransformationAttribute

There's `HtmlTransformationAttribute` defined. It has `Order` field which might be used to manipulate the order of transformation applied (a lower number means higher precedence).
Use this attribute to annotate your class implementing `IHtmlTransformation`.

NOTE: you **don't** need `HtmlTransformation` attribute in order for transformation to be applied. It's only used for order manipulation purposes. 
