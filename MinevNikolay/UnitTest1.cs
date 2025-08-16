namespace ExamNikolayMinev;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using static System.Net.WebRequestMethods;

[TestFixture]
public class Tests
{
    private RestClient client;
    private static string createdStoryId;
    private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
    [OneTimeSetUp]
    public void Setup()
    {
        string token = GetJwtToken("Nikolay1", "Nikolay123456");

        var option = new RestClientOptions(baseUrl)
        {
            Authenticator = new JwtAuthenticator(token)
        };

        client = new RestClient(option);
    }

    private string GetJwtToken(string username, string password)
    {
        var loginClien = new RestClient(baseUrl);

        var request = new RestRequest("/api/User/Authentication", Method.Post);

        request.AddJsonBody(new { username, password });

        var response = loginClien.Execute(request);

        var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
        return json.GetProperty("accessToken").GetString() ?? string.Empty;
    }

    [Test, Order(1)]
    public void CreatNewStory_ShouldReturnCreatedStory()
    {
        var newStory = new
        {
            Title = "My new Story",
            Description = "This is good story",
            Url = ""
        };

        var reqest = new RestRequest("api/Story/Create", Method.Post);

        reqest.AddJsonBody(newStory);

        var response = client.Execute(reqest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
        createdStoryId = json.GetProperty("storyId").GetString();
        Assert.IsFalse(string.IsNullOrEmpty(createdStoryId), "StoryId should not be null or empty");
        Assert.That(response.Content, Does.Contain("Successfully created!"));
    }
    [Test, Order(2)]
    public void EditStorySpoilerThatWasCreated_ShouldBeEdited() 
    {

       var change = new
        {
             Title = "Changed story",
             Description = "Changed description",
             Url = ""


        };
        var request = new RestRequest($"api/Story/Edit/{createdStoryId}", Method.Put);
        request.AddJsonBody(change);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code should be 200 OK");
        Assert.That(response.Content, Does.Contain("Successfully edite"));


    }
    [Test, Order(3)]
    public void GetAllStorys_ShouldReturnList()
    {
        var request = new RestRequest("/api/Story/All", Method.Get);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code should be 200 OK");
        var story = JsonSerializer.Deserialize<List<object>>(response.Content);
        Assert.That(story, Is.Not.Empty);
    }

    [Test, Order(4)]
    public void DeleteStory_ShouldDeletedTheStory() 
    {
        var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code should be 200 OK");
        Assert.That(response.Content, Does.Contain("Deleted successfully!"));
    }

    [Test, Order(5)]
    public void CreatingTheStroyWihoutTitleAndDescription_ShouldReturnBadRequest() 
    {
        var secondStory = new
        {
            Title = "",
            Description = "",
            Url = ""
            
    };
        var reqest = new RestRequest("api/Story/Create", Method.Post);
        reqest.AddJsonBody(secondStory);

        var response = client.Execute(reqest);

        Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.BadRequest));

    }
    [Test, Order(6)]
    public void EditNonExsitingStory_ShouldReturnNotFound() 
    {
        string fakeId = "223";
        var change = new
        {
            Title = "Changed story",
            Description = "Changed description",
            Url = ""
        };
        var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
        request.AddJsonBody(change);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

    }
    [Test, Order(7)]
    public void DeleteNonExsitnigStory_ShouldReturnBadRequest() 
    {
        string fakeId = "321";
        var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler"));
    }



    [OneTimeTearDown]
    public void Cleanup()
    {

        client?.Dispose();

    }

}
