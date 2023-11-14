namespace PatchPractice
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }

        [PatchNotAllowed]
        public string? Gender { get; set; }
    }

    public class PersonPatchVM
    {
        public string? Name { get; set; }
        public int? Age { get; set; }

        [PatchNotAllowed]
        public string? Gender { get; set; }
    }



    public class PatchNotAllowedAttribute : Attribute { }
}
