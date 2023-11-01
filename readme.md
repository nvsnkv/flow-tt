# Flow TT - a plugin for custom CSVs import

Plugin allows to transform a custom CSV, skip invalid records and map each valid row to an `IncomingTransaction` instance.

## Usage
Just copy TT binaries into separate folder with _flow_ plugins and start the app. TT will read its own configuration and provide readers for custom formats. These readers will be available for `flow tx add` and `flow import start` operations.

## Configuration
TT uses json configuration file, that contains set of formats with transforamtion, valiation and mapping rules for each format.
```
{
    "Formats": {
        "custom-csv": {
            "Transform": {
                0: { "Pattern": "USD", "Replacement": "$" },
                1: ...
            },
            "SkipIf": {
                2: { "Pattern": "FAIL" }
            },
            "Mapping": {
                0: "Currency"
                1: "Amount"
                ...
            }
        }
    }
}
```