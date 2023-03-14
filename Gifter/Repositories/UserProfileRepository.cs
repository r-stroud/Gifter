using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Gifter.Models;
using Gifter.Utils;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;

namespace Gifter.Repositories
{
    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        public UserProfileRepository(IConfiguration configuration) : base(configuration) { }

        public List<UserProfile> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT Id, Name, Email, ImageUrl, Bio, DateCreated 
FROM UserProfile
ORDER BY DateCreated";

                    var reader = cmd.ExecuteReader();
                    var profiles = new List<UserProfile>();
                    while (reader.Read())
                    {
                        profiles.Add(new UserProfile()
                        {
                            Id = DbUtils.GetInt(reader, "Id"),
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            ImageUrl = DbUtils.GetString(reader, "imageUrl"),
                            Bio = DbUtils.GetString(reader, "Bio"),
                            DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                        });
                    }
                    reader.Close();
                    return profiles;
                }
            }
        }

        public UserProfile GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT Name, Email, ImageUrl, Bio, DateCreated 
FROM UserProfile
WHERE Id = @id";
                    DbUtils.AddParameter(cmd, "@id", id);    

                    var reader = cmd.ExecuteReader();

                    UserProfile profile = null;

                    if (reader.Read())
                    {
                        profile = new UserProfile()
                        {
                            Id = id,
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            ImageUrl = DbUtils.GetString(reader, "imageUrl"),
                            Bio = DbUtils.GetString(reader, "Bio"),
                            DateCreated = DbUtils.GetDateTime(reader, "DateCreated")

                        };
                    }

                    reader.Close();
                    return profile;
                }
            }
        }
        
        public UserProfile GetByIdWithPosts(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 

UP.Name, UP.Email, UP.ImageUrl AS UserProfileImageUrl, UP.Bio, UP.DateCreated AS UserProfileDateCreated,

P.ID AS PostId, P.Title, P.ImageUrl AS PostImageUrl, P.Caption, P.DateCreated AS PostDateCreated, P.UserProfileId AS PostUserProfileId,

C.Id AS CommentId, C.Message, C.UserProfileId As CommentUserProfileId

FROM UserProfile UP
LEFT JOIN Post P
ON P.UserProfileId = UP.Id
LEFT JOIN Comment C
ON C.PostId = P.Id
WHERE UP.Id = @id";

                    DbUtils.AddParameter(cmd, "@id", id);

                    var reader = cmd.ExecuteReader();

                    UserProfile profile = null;

                    while (reader.Read())
                    {
                        if (profile == null)
                        {
                            profile = new UserProfile()
                            {
                                Id = id,
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                Bio = DbUtils.GetString(reader, "Bio"),
                                DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                Posts = new List<Post>()

                            };

                        }

                        if (DbUtils.IsNotDbNull(reader, "PostId"))
                        {
                            var postId = DbUtils.GetInt(reader, "PostId");


                            var existingPost = profile.Posts.FirstOrDefault(p => p.Id == postId);
                            if (existingPost == null)
                            {
                                existingPost = new Post()
                                {
                                    Id = postId,
                                    Title = DbUtils.GetString(reader, "Title"),
                                    Caption = DbUtils.GetString(reader, "Caption"),
                                    DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                                    UserProfileId = DbUtils.GetInt(reader, "PostUserProfileId"),
                                    Comments = new List<Comment>()

                                };

                                profile.Posts.Add(existingPost);

                            }

                            if (DbUtils.IsNotDbNull(reader, "CommentId"))
                            {
                                existingPost.Comments.Add(new Comment()
                                {
                                    Id = DbUtils.GetInt(reader, "CommentId"),
                                    Message = DbUtils.GetString(reader, "Message"),
                                    PostId = postId,
                                    UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                                });
                            }
                        };
                    }
                    
                    reader.Close();
                    return profile;
                }
            }
        }

        public void Add(UserProfile userProfile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT INTO UserProfile (Name, Email, ImageUrl, Bio, DateCreated)
OUTPUT INSERTED.Id
VALUES (@Name, @Email, @ImageUrl, @Bio, @DateCreated)";

                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@Bio", userProfile.Bio);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);

                    userProfile.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(UserProfile userProfile) 
        {
            using (var conn = Connection)
            {
                conn.Open();
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
UPDATE UserProfile
SET Name = @Name,
Email = @Email,
ImageUrl = @ImageUrl,
Bio = @Bio,
DateCreated = @DateCreated
WHERE ID =@Id";
                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@Bio", userProfile.Bio);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);
                    DbUtils.AddParameter(cmd, "@Id", userProfile.Id);

                    cmd.ExecuteNonQuery();

                }
            }
        
        }

        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using(var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
DELETE FROM UserProfile
WHERE Id = @id";
                    DbUtils.AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();

                }
            }
        }


    }
}
