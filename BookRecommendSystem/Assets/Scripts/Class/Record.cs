
public class Record
{
    public string ISBN;
    public string bookName;
    public string bookIntro;
    public int bookImage;
    public string pressName;
    public string pressCity;
    public string pressYear;

    public Record() { }

    public Record(string ISBN, string bookName, string pressName, string pressCity, string pressYear)
    {
        this.ISBN = ISBN;
        this.bookName = bookName;
        this.pressName = pressName;
        this.pressCity = pressCity;
        this.pressYear = pressYear;
    }
}
