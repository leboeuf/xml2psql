# xml2psql

This utility takes an XML file as input and executes psql commands to create the schema described in it.

## Usage

Usage: `xml2psql ./path-to-schema-xml-file [./path-to-seed-sql-file]`

The optional seed file is an SQL file that will be executed as-is after the schema has been created. The XML file must have a structure similar to the following:

```
<xml2psql>
    <ConnectionString>Host=localhost;Port=5432;Database=xmltest;Username=xmltest_user;Password=test</ConnectionString>
    <Schema>
        <Tables>
            <Table name="brands">
                <Column name="id" type="uuid" isprimarykey="true"/>
                <Column name="name" type="text" unique="true" notnull="true"/>
            </Table>
            <Table name="products">
                <Column name="id" type="uuid" isprimarykey="true"/>
                <Column name="name" type="text" unique="true" notnull="true"/>
                <Column name="brand_id" type="uuid" hasindex="true" foreignkey="brands (id)"/>
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

Note: tables referenced by foreign keys must appear before being referenced.

## Docker

To quickly spin up a PostgreSQL instance, run the following command:

`docker run --name postgres -e POSTGRES_PASSWORD=test -p 5432:5432 -d postgres`

This will be reachable from the host with this connection string: `Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=test`

If using UUID columns, run this before running xml2psql: `docker exec -it <container-id> psql -U postgres -d postgres -c 'CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";'`
