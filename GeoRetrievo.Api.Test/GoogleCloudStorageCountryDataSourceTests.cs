namespace GeoRetrievo.Api.Test;

public class GoogleCloudStorageCountryDataSourceTests {
    const string BUCKET = "countries-bucket";
    const string OBJECT_NAME = "data/countries.json";
    const string PAYLOAD = "[{\"name\":\"Canada\",\"code\":\"CA\"}]";

    [Fact]
    public async Task DownloadAsyncDownloadsObjectAndResetsStreamPosition() {

        var dataSource = new GoogleCloudStorageCountryDataSource((actualBucket, actualObjectName, cancellationToken) => {
                actualBucket.ShouldBe(BUCKET);
                actualObjectName.ShouldBe(OBJECT_NAME);
                cancellationToken.ShouldBe(TestContext.Current.CancellationToken);

                var content = new MemoryStream(Encoding.UTF8.GetBytes(PAYLOAD));
                content.Position = content.Length;
                return Task.FromResult<Stream>(content);
            });

        await using var downloaded = await dataSource.DownloadAsync(BUCKET, OBJECT_NAME, TestContext.Current.CancellationToken);

        downloaded.Position.ShouldBe(0);

        using var reader = new StreamReader(downloaded, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        (await reader.ReadToEndAsync(TestContext.Current.CancellationToken)).ShouldBe(PAYLOAD);
    }
}