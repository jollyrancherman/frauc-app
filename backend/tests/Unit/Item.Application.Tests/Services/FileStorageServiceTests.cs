using FluentAssertions;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Marketplace.Application.Items.Services;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Application.Common;

namespace Item.Application.Tests.Services;

public class FileStorageServiceTests
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageServiceTests()
    {
        _logger = Substitute.For<ILogger<FileStorageService>>();
        _fileStorageService = new FileStorageService(_logger);
    }

    [Fact]
    public async Task UploadImageAsync_WithValidImage_ShouldReturnImageUrl()
    {
        // Arrange
        var formFile = CreateMockImageFile("test.jpg", 1024);
        
        // Act
        var result = await _fileStorageService.UploadImageAsync(formFile, "items", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Should().StartWith("https://");
    }

    [Fact]
    public async Task UploadImageAsync_WithInvalidFileType_ShouldFail()
    {
        // Arrange
        var formFile = CreateMockFile("test.txt", 1024, "text/plain");
        
        // Act
        var result = await _fileStorageService.UploadImageAsync(formFile, "items", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid file type");
    }

    [Fact]
    public async Task UploadImageAsync_WithFileTooLarge_ShouldFail()
    {
        // Arrange
        var formFile = CreateMockImageFile("test.jpg", 10 * 1024 * 1024); // 10MB
        
        // Act
        var result = await _fileStorageService.UploadImageAsync(formFile, "items", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("File size exceeds maximum");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public async Task UploadImageAsync_WithEmptyContainer_ShouldFail(string container)
    {
        // Arrange
        var formFile = CreateMockImageFile("test.jpg", 1024);
        var actualContainer = container == "null" ? null! : container;
        
        // Act
        var result = await _fileStorageService.UploadImageAsync(formFile, actualContainer, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Container name cannot be empty");
    }

    [Fact]
    public async Task DeleteImageAsync_WithValidUrl_ShouldSucceed()
    {
        // Arrange - Use the same base URL that FileStorageService uses
        var imageUrl = "https://localhost:7001/uploads/items/test-image.jpg";
        
        // Act
        var result = await _fileStorageService.DeleteImageAsync(imageUrl, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private static IFormFile CreateMockImageFile(string fileName, long sizeInBytes)
    {
        return CreateMockFile(fileName, sizeInBytes, "image/jpeg");
    }

    private static IFormFile CreateMockFile(string fileName, long sizeInBytes, string contentType)
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.Length.Returns(sizeInBytes);
        formFile.ContentType.Returns(contentType);
        
        var stream = new MemoryStream(new byte[sizeInBytes]);
        formFile.OpenReadStream().Returns(stream);
        
        return formFile;
    }
}