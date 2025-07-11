
namespace LMS.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
    }

    public class Book : Entity
    {
        private Book() { }
        public Book(string title, string author, string code, string publisher, string genre,
            int pages, int releasedYear, int totalCopies)
        {
            Title = title;
            Author = author;
            Code = code;
            Publisher = publisher;
            Genre = genre;

            Pages = pages;
            ReleasedYear = releasedYear;
            TotalCopies = totalCopies;
            AvailableCopies = totalCopies;

            IsActive = true;
        }

        public void SetInactive() => IsActive = false;
        public void SetActive() => IsActive = true;

        public void IncrementAvailableCopies()
        {
            if (AvailableCopies < TotalCopies) AvailableCopies++;

            TotalNumberOfLendings++;
        }

        public void DecrementAvailableCopies()
        {
            if (AvailableCopies > 0) AvailableCopies--;
        }

        public string Title { get; private set; }
        public string Author { get; private set; }
        public string Code { get; private set; }
        public string Publisher { get; private set; }
        public string Genre { get; private set; }
        public int Pages { get; private set; }
        public int ReleasedYear { get; private set; }
        public int TotalCopies { get; private set; }
        public int AvailableCopies { get; private set; }
        public int TotalNumberOfLendings { get; private set; }

        public bool IsActive { get; private set; }

        public virtual ICollection<UserBookLending> UserBookLendings { get; private set; } = [];
    }

    public class User : Entity
    {
        private User() { }

        public User(string firstName, string lastName,
            DateTime birthDate,
            string phoneNumber, string email,
            string address)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
            PhoneNumber = phoneNumber;
            Email = email;
            Address = address;
            IsActive = true;
        }

        public void SetInactive() => IsActive = false;
        public void SetActive() => IsActive = true;

        public void UpdatePhoneNumber(string phoneNumber) => PhoneNumber = phoneNumber;
        public void UpdateEmail(string email) => Email = email;

        public void IncreamentLendingBookCount() => LendingBookCount++;
        public void DecreamentLendingBookCount() => LendingBookCount--;

        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public DateTime BirthDate { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Email { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }
        public int LendingBookCount { get; private set; }

        public virtual ICollection<UserBookLending> UserBookLendings { get; private set; } = [];
    }


    public class UserBookLending : Entity
    {

        private UserBookLending()
        {

        }

        public UserBookLending(Guid bookId, Guid userId, DateTime lendingDate)
        {
            BookId = bookId;
            UserId = userId;
            LendingDate = lendingDate;
        }

        public void UpdateSubmittedDate(DateTime submittedDate, string remarks)
        {
            SubmittedDate = submittedDate;

            if (!string.IsNullOrEmpty(remarks))
                Remarks = remarks;
        }

        public Guid BookId { get; private set; }
        public Guid UserId { get; private set; }

        public DateTime LendingDate { get; private set; }
        public DateTime? SubmittedDate { get; private set; }
        public string Remarks { get; private set; }

        public virtual Book Book { get; private set; }
        public virtual User User { get; private set; }
    }

}
