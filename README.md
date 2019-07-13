# Moq.Dapper
Moq extensions for Dapper methods.

[![NuGet Version and Downloads count](https://buildstats.info/nuget/Moq.Dapper)](https://www.nuget.org/packages/Moq.Dapper)
[![](https://dev.azure.com/unosd/Moq.Dapper/_apis/build/status/Publish%20to%20NuGet)]()

# Example usage

Mocking a call to `Query` with a simple type:

```csharp
[Test]
public void QueryGeneric()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[] { 7, 77, 777 };

    connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
              .Returns(expected);

    var actual = connection.Object.Query<int>("", null, null, true, null, null).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
}
```

Mocking a call to `Query` with a complex type:

```csharp
[Test]
public void QueryGenericComplexType()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[]
    {
        new ComplexType { StringProperty = "String1", IntegerProperty = 7 },
        new ComplexType { StringProperty = "String2", IntegerProperty = 77 },
        new ComplexType { StringProperty = "String3", IntegerProperty = 777 }
    };

    connection.SetupDapper(c => c.Query<ComplexType>(It.IsAny<string>(), null, null, true, null, null))
              .Returns(expected);

    var actual = connection.Object.Query<ComplexType>("", null, null, true, null, null).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));

    foreach (var complexObject in expected)
    {
        var match = actual.Where(co => co.StringProperty == complexObject.StringProperty && co.IntegerProperty == complexObject.IntegerProperty);

        Assert.That(match.Count, Is.EqualTo(1));
    }
}
```

Mocking a call to `ExecuteScalar`:

```csharp
[Test]
public void ExecuteScalar()
{
    var connection = new Mock<IDbConnection>();

    const int expected = 77;

    connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
              .Returns(expected);

    var actual = connection.Object.ExecuteScalar<int>("", null, null, null);

    Assert.That(actual, Is.EqualTo(expected));
}
```

Mocking a call to `QueryAsync`

```csharp
[Test]
public async Task QueryAsyncGeneric()
{
    var connection = new Mock<DbConnection>();

    var expected = new[] { 7, 77, 777 };

    connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
              .ReturnsAsync(expected);

    var actual = (await connection.Object.QueryAsync<int>("", null, null, true, null, null)).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
}
```
