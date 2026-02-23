using Innowise.Music.Model;

namespace Innowise.Music.Services;

public class MockNewsService : INewsService
{
    readonly List<News> _newsList = new()
        {
            new News() { Id= 1, Title = "Title1", Content= "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum", ImageUrl= "https://www.pewresearch.org/wp-content/uploads/sites/8/2016/07/PJ_2016.07.07_Modern-News-Consumer_0-01.png" },
            new News() { Id= 2, Title = "Title2", Content="Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum", ImageUrl= "https://talkbusiness.net/wp-content/uploads/2020/09/digital-news.png" },
            new News() { Id= 3, Title = "Title3",Content="Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum", ImageUrl="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPstMdHYFqFpMAqFjockxMki33yORhGPT02w&usqp=CAU" },
        };

    public List<News> GetNews()
    {
        return _newsList;
    }

    public Task<List<News>> GetNewsAsync()
    {
        throw new NotImplementedException();
    }
}