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
            "Mapping": {
                "Timestamp": 0,
                "Amount": 1,
                "Currency": 2,
                "Category": 3,
                "Title": 4,
                "AccountName": 5,
                "AccountBank": 6,
                "OverridesComment": 7,
                "OverridesCategory": 8,
                "OverridesTitle": 9,
                "Any Extra fields needed for validation": 20
            },
            "SkipIf": {
                "Any Extra fields needed for validation": { "Pattern": "FAIL" }
            },
            "Transform": {
                "Currency": { "Pattern": "USD", "Replacement": "$" },
                "OverridesComment": {"Pattern": "^(\d{4})$", "Replacement: "MCC $1"}
            }
            
        }
    }
}
```
`Mapping` defines column-to-field map. `SkipIf` allows to ignore some transcations, like failed or pending ones, that match specified regular expression pattern. `Transform` allows to change input string using regular expressions.

Single config file can contain multiple formats - the names should be unique.