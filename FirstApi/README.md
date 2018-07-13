# PLURALSIGHT Course - Building Your First API with ASP.NET Core 

The purpose of this repository is to present the final project of the course. And the purpose of this course is to teach how to construct a REST API with ASP.NET Core framework from scratch.

## Description

The API serves a list of cities and their points of interest. The client can interact with the API consulting cities and points of interest,creating points of interest, updating their information and deleting them. This REST API has the **Level 2** in [Richardson Maturity Model](https://martinfowler.com/articles/richardsonMaturityModel.html).

### REST API end points

Source URL: `http://localhost:5000`

| Verb | URI | Description |
| ---- | --- | ----------- |
| GET | `/api/cities` | List all cities |
| GET | `/api/cities/{cityId}` | Shows a city based on their `cityId` |
| GET | `/api/cities/{cityId}/pointsofinterest` | Shows all points of interest of a city |
| GET | `/api/cities/{cityId}/pointsofinterest/{id}` | Shows a point of interest of a city based on their `cityId` and `id` |
| POST | `/api/cities/{cityId}/pointsofinterest` | Create a new point of interest. See the body model 1 |
| PUT | `/api/cities/{cityId}/pointsofinterest/{id}` | Update an existing point of interest. See the body model 1 |
| PATCH | `/api/cities/{cityId}/pointsofinterest/{id}` | Partialy update an existing point of interest. See the body model 2 |

#### Model 1
```
{
    "Name": "Point of Interest Name",
    "Description": "Point of Interest Description"
}
```

#### Model 2
Model like the JavaScript Object of [RFC6902](https://tools.ietf.org/html/rfc6902)

```
[
    {
        "op": "replace",
        "path": "/property",
        "value": "Updated - property"
    }
]
```

To start the server, first enter the project directory:
```
cd FirstApi
```

Then run the dotnet command:
```
dotnet run
```

## Course Detail

| | |
| --- | --- |
| Source | [Pluralsight](http://www.pluralsight.com/) |
| Name | [Building Your First API with ASP.NET Core] (https://app.pluralsight.com/library/courses/asp-dotnet-core-api-building-first/description) |
| Author | Kevin Dockx |
| Level | Beginner |

## Environment Detail

| | |
| - | - |
| OS | Windows 10 Home Single Language |
| Type | 64 bits |
| Processor | i7-7500U CPU @ 2.70GHz 2.90GHz |
| RAM | 16,0 GB |
| IDE | Microsoft Visual Studio Community 2017 v15.7.4 |
| | Microsoft .NET Framework v4.7.02046 |
| | SQL Server Data Tools v15.1.61804.210 |