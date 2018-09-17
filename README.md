# xml2psql

This utility takes an XML file as input and executes psql commands to create the schema described in it.

## Usage

Usage: `xml2psql ./path-to-xml-file`

The XML file must have a structure similar to the following:

```
<xml2psql>
    <ConnectionString></ConnectionString>
    <Schema>
        <Tables>
            <Table>
                <Column/>
                <Column/>
                <Column/>
                <Index/>
            </Table>
        </Tables>
    </Schema>
</xml2psql>
```
