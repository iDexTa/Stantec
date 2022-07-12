namespace StantecSearchWebApp.Models;

public class HomeViewModel
{
    public List<People> People {get; set;}

    public int peopleCount {get; set;}

    public string SearchColumn {get; set;}

    public string SearchString {get; set;}

    public string[] AutoCompleteArray {get; set;}
}