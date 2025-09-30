using AutoFixture;
using Domain.Entities;

using TopicService.Tests.AutoData; 

namespace TopicService.Tests.AutoData.Customizations;

public class ValidInputsCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Topic>(composer =>
        {
            return composer.FromFactory(() =>
            {
                var topicName = fixture.CreateString(15);
                var userId = fixture.Create<Guid>();

                return Topic.Create(topicName, userId);
            });
        });
        
        fixture.Customize<Post>(composer =>
        {
            return composer
                .FromFactory(() =>
                {
                    var topic = fixture.Create<Topic>();
                    var userId = fixture.Create<Guid>();
                    var title = fixture.CreateString(10);
                    var text = fixture.CreateString(30);

                    return Post.Create(title, text, userId, topic.Id);
                });
        });

        fixture.Customize<Comment>(composer =>
        {
            return composer.FromFactory(() =>
            {
                var post = fixture.Create<Post>();
                var userId = fixture.Create<Guid>();
                var text = fixture.CreateString(200);

                return Comment.Create(text, userId, post.Id);
            });
        });
    }
}
