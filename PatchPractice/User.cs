namespace PatchPractice
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
    }

    public class UserPatchVM
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
    }
}
