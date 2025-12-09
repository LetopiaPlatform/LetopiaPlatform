using Bokra.Core.Common;
using Bokra.Core.DTOs.UserProfile.Request;
using Bokra.Core.Entities.Identity;
using Bokra.Core.Interfaces;
using Bokra.Infrastructure.Data;
using Bokra.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Bokra.UnitTests
{

    public class UserServicesTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IGenericRepository<User>> _repoMock;
        private readonly Mock<IUnitOfWork<ApplicationDbContext>> _unitOfWorkMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly UserServices _service;

        public UserServicesTests()
        {
            _userManagerMock = MockUserManager();
            _repoMock = new Mock<IGenericRepository<User>>();
            _unitOfWorkMock = new Mock<IUnitOfWork<ApplicationDbContext>>();
            _fileStorageMock = new Mock<IFileStorageService>();

            _service = new UserServices(
                _userManagerMock.Object,
                _repoMock.Object,
                _unitOfWorkMock.Object,
                _fileStorageMock.Object
            );
        }

        // Helper: Fake UserManager
        private static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        // ============================
        //  GetProfileAsync Tests
        // ============================

        [Fact]
        public async Task GetProfileAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((User)null);

            var result = await _service.GetProfileAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnProfile_WhenUserExists()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Ahmed",
                Bio = "Developer",
                AvatarUrl = "avatar.jpg",
                Role = "User",
                TotalPoints = 100
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                            .ReturnsAsync(user);

            var result = await _service.GetProfileAsync(user.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ahmed", result.Value.FullName);
        }

        // ============================
        //  UpdateProfileAsync Tests
        // ============================

        [Fact]
        public async Task UpdateProfileAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((User)null);

            var req = new UpdatedProfileRequest(
                Id: Guid.NewGuid(),
                FullName: "Test",
                Bio: "Test bio",
                DateOfBirth: null
            );

            var result = await _service.UpdateProfileAsync(req);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }


        [Fact]
        public async Task UpdateProfileAsync_ShouldUpdateUser_WhenUserExists()
        {
            var user = new User { Id = Guid.NewGuid() };

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                            .ReturnsAsync(user);

            var req = new UpdatedProfileRequest
            (
                Id : user.Id,
                Bio : "New Bio",
                FullName : "New Name",
                DateOfBirth : new DateTime(2000, 1, 1)
            );

            var result = await _service.UpdateProfileAsync(req);

            _repoMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );

            Assert.True(result.IsSuccess);
        }

        // ============================
        //  UploadAvatarAsync Tests
        // ============================

        [Fact]
        public async Task UploadAvatarAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                            .ReturnsAsync((User)null);

            var req = new AvatarRequest(
                UserId: Guid.NewGuid(),
                Avatar: null
            );

            var result = await _service.UploadAvatarAsync(req);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }


        [Fact]
        public async Task UploadAvatarAsync_ShouldFail_WhenAvatarIsEmpty()
        {
            var user = new User { Id = Guid.NewGuid() };

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                            .ReturnsAsync(user);

            var req = new AvatarRequest
            (
                UserId : user.Id,
                Avatar : null
            );

            var result = await _service.UploadAvatarAsync(req);

            Assert.False(result.IsSuccess);
            Assert.Equal(new List<string> { "No avatar file uploaded." }, result.Errors);

        }

        [Fact]
        public async Task UploadAvatarAsync_ShouldUploadAndSave_WhenValid()
        {
            var user = new User { Id = Guid.NewGuid() };

            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(100);

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                            .ReturnsAsync(user);

            _fileStorageMock.Setup(x => x.UploadAsync(file.Object, "avatars"))
                            .ReturnsAsync(Result<string>.Success("uploaded.png"));

            var req = new AvatarRequest
            (
                UserId : user.Id,
                Avatar : file.Object
            );

            var result = await _service.UploadAvatarAsync(req);

            Assert.True(result.IsSuccess);
            Assert.Equal("uploaded.png", result.Value);

            _repoMock.Verify(x => x.UpdateAsync(user), Times.Once); 
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }

}
