# xml2psql

This utility takes an XML file as input and executes psql commands to create the schema described in it.

## Usage

Usage: `xml2psql ./path-to-xml-file`

The XML file must have a structure similar to the following:

```
<xml2psql>
    <ConnectionString>Host=localhost;Port=5432;Database=xmltest;Username=xmltest_user;Password=test</ConnectionString>
    <Schema>
        <Tables>
            <Table name="products">
                <Column name="id" type="uuid" isprimarykey="true"/>
                <Column name="name" type="text" unique="true" notnull="true"/>
                <Column name="brand_id" type="uuid" hasindex="true"/>
            </Table>
            <Table name="users">
                <Column name="id" type="uuid" isprimarykey="true"/>
                <Column name="username" type="text" unique="true" notnull="true"/>
                <Column name="password_hash" type="text" notnull="true"/>
            </Table>
        </Tables>
    </Schema>
</xml2psql>
```
