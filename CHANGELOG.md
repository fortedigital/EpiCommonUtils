# Changelog

## 3.0.x (2021-07-30)

### Changes

- `EnumSelectionFactory` returns `SelectItem` with enum values casted to the underlying type before being boxed. It might be a breaking change for anyone using custom json converters for their enum types as right now, these converters will be ignored when sending `SelectItem` to EpiServer UI.
