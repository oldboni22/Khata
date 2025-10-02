using System;
using AutoFixture;
using Domain.Entities;
using Domain.Exceptions;
using Shared.Exceptions;
using Shouldly;
using TopicService.Tests.AutoData;
using Xunit;
using Xunit.Sdk;

namespace TopicService.Tests;

public class TopicEntityTests
{
    [Theory]
    [AutoDomainData]
    public void AddSubTopic_ValidInput_CreatesSubTopic(IFixture fixture, Topic parentTopic, Guid creatorId)
    {
        var subTopicName = fixture.CreateString(15);
        
        // Act
        var subTopic = parentTopic.AddSubTopic(subTopicName, creatorId);
        
        // Assert
        parentTopic.SubTopics.ShouldContain(subTopic);
        subTopic.Name.ShouldBe(subTopicName);
        subTopic.ParentTopicId.ShouldBe(parentTopic.Id);
        subTopic.OwnerId.ShouldBe(creatorId);
    }
    
    [Theory]
    [AutoDomainData]
    public void AddSubTopic_InvalidInput_Throws(IFixture fixture, Topic parentTopic, Guid creatorId)
    {
        var subTopicName = fixture.CreateString(30);
        
        // Act & Assert
        Should.Throw<ArgumentException>(() => parentTopic.AddSubTopic(subTopicName, creatorId));
    }
    
    [Theory]
    [AutoDomainData]
    public void RemoveSubTopic_TopicExistsAnParentTopicOwner_RemovesSubTopic(Topic parentTopic)
    {
        // Arrange
        var senderId = parentTopic.OwnerId;
        var subTopic = parentTopic.AddSubTopic("ValidName", senderId);
        
        // Act
        parentTopic.RemoveSubTopic(subTopic.Id, senderId, false);
        
        // Assert
        parentTopic.SubTopics.ShouldNotContain(subTopic);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemoveSubTopic_TopicExistsAndModerator_RemovesSubTopic(Topic parentTopic)
    {
        // Arrange
        var senderId = parentTopic.OwnerId;
        var subTopic = parentTopic.AddSubTopic("ValidName", senderId);
        
        // Act
        parentTopic.RemoveSubTopic(subTopic.Id, senderId, true);
        
        // Assert
        parentTopic.SubTopics.ShouldNotContain(subTopic);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemoveSubTopic_TopicExistsAndSubTopicOwner_RemovesSubTopic(Topic parentTopic)
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var subTopic = parentTopic.AddSubTopic("ValidName", senderId);
        
        // Act
        parentTopic.RemoveSubTopic(subTopic.Id, senderId, true);
        
        // Assert
        parentTopic.SubTopics.ShouldNotContain(subTopic);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemoveSubTopic_TopicDontExist_ThrowsNotFound(Topic parentTopic)
    {
        // Act && Assert
        Should.Throw<EntityNotFoundException<Topic>>(
            () => parentTopic.RemoveSubTopic(Guid.NewGuid(), parentTopic.OwnerId, false));
    }
    
    [Theory]
    [AutoDomainData]
    public void RemoveSubTopic_TopicExistsAndInvalidUser_ThrowsForbidden(Topic parentTopic)
    {
        // Arrange
        var senderId = parentTopic.OwnerId;
        var subTopic = parentTopic.AddSubTopic("ValidName", senderId);
        
        // Act && Assert
        Should.Throw<ForbiddenException>(
            () => parentTopic.RemoveSubTopic(subTopic.Id, Guid.NewGuid(), false));
    }
    
    [Theory]
    [AutoDomainData]
    public void AddPost_ValidInput_CreatesPost(IFixture fixture, Topic topic, Guid authorId)
    {
        var title = fixture.CreateString(10);
        var text = fixture.CreateString(30);
        
        // Act
        var post = topic.AddPost(title, text, authorId);
        
        // Assert
        topic.Posts.ShouldContain(post);
        post.Title.ShouldBe(title);
        post.Text.ShouldBe(text);
        post.TopicId.ShouldBe(topic.Id);
        post.AuthorId.ShouldBe(authorId);
        topic.PostCount.ShouldBe(1);
    }
    
    [Theory]
    [AutoDomainData]
    public void AddPost_InvalidTitle_Throws(IFixture fixture, Topic topic, Guid authorId)
    {
        var title = fixture.CreateString(50);
        var text = fixture.CreateString(30);
        
        // Act & Assert
        Should.Throw<ArgumentException>(() => topic.AddPost(title, text, authorId));
    }
    
    [Theory]
    [AutoDomainData]
    public void AddPost_InvalidText_Throws(IFixture fixture, Topic topic, Guid authorId)
    {
        var title = fixture.CreateString(50);
        var text = fixture.CreateString(1501);
        
        // Act & Assert
        Should.Throw<ArgumentException>(() => topic.AddPost(title, text, authorId));
    }
    
    [Theory]
    [AutoDomainData]
    public void RemovePost_TopicExistsAndTopicOwner_RemovesPost(Topic topic, Guid authorId)
    {
        // Arrange
        var post = topic.AddPost("Post Title", "Post Text", authorId);
        var initialPostCount = topic.PostCount;
        var senderId = topic.OwnerId; // Отправитель - владелец топика

        // Act
        topic.RemovePost(post.Id, senderId, false);

        // Assert
        topic.Posts.ShouldNotContain(post);
        topic.PostCount.ShouldBe(initialPostCount - 1);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemovePost_TopicExistsAndPostAuthor_RemovesPost(Topic topic)
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var post = topic.AddPost("Post Title", "Post Text", authorId);
        var initialPostCount = topic.PostCount;
        var senderId = authorId; // Отправитель - автор поста

        // Act
        topic.RemovePost(post.Id, senderId, false);

        // Assert
        topic.Posts.ShouldNotContain(post);
        topic.PostCount.ShouldBe(initialPostCount - 1);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemovePost_TopicExistsAndModerator_RemovesPost(Topic topic, Guid authorId)
    {
        // Arrange
        var post = topic.AddPost("Post Title", "Post Text", authorId);
        var initialPostCount = topic.PostCount;
        var senderId = Guid.NewGuid(); // Произвольный пользователь, но с правами модератора

        // Act
        topic.RemovePost(post.Id, senderId, true); // isMod = true

        // Assert
        topic.Posts.ShouldNotContain(post);
        topic.PostCount.ShouldBe(initialPostCount - 1);
    }
    
    [Theory]
    [AutoDomainData]
    public void RemovePost_TopicExistsAndInvalidUser_ThrowsForbiddenException(Topic topic, Guid authorId)
    {
        // Arrange
        var post = topic.AddPost("Post Title", "Post Text", authorId);
        var invalidSenderId = Guid.NewGuid(); // Посторонний пользователь

        // Act & Assert
        Should.Throw<ForbiddenException>(
            () => topic.RemovePost(post.Id, invalidSenderId, false));
    }
    
    [Theory]
    [AutoDomainData]
    public void RemovePost_PostDoesNotExist_ThrowsNotFound(Topic topic)
    {
        // Arrange
        var nonExistentPostId = Guid.NewGuid();
        var senderId = topic.OwnerId;

        // Act & Assert
        Should.Throw<EntityNotFoundException<Post>>(
            () => topic.RemovePost(nonExistentPostId, senderId, false));
    }
}
