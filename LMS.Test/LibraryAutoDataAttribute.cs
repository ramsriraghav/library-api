using AutoFixture;
using AutoFixture.Xunit2;
using LMS.Domain.Entities;
using System.Reflection;

namespace LMS.Test
{
    public class LibraryAutoDataAttribute : AutoDataAttribute
    {
        public LibraryAutoDataAttribute() : base(() =>
        {
            var fixture = new Fixture();

            // Customize UserBookLending
            fixture.Customize<UserBookLending>(c => c
                .Do(l =>
                {
                    var book = fixture.Create<Book>();
                    var user = fixture.Create<User>();
                    typeof(Entity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(l, Guid.NewGuid());
                    typeof(UserBookLending).GetProperty("Book", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(l, book);
                    typeof(UserBookLending).GetProperty("User", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(l, user);
                    typeof(UserBookLending).GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance)
                       ?.SetValue(l, user.Id);
                    typeof(UserBookLending).GetProperty("BookId", BindingFlags.Public | BindingFlags.Instance)
                     ?.SetValue(l, book.Id);
                }));

            return fixture;
        })
        {
        }
    }
}