open System
open System.Windows.Forms
open System.Drawing
open System.IO
open System.Text.Json

type DictionaryEntry = {
    Word: string
    Definition: string
}

type MainForm() as this =
    inherit Form()

    let dictionary = ref Map.empty<string, string>
    let mutable currentFile = "dictionary.json"

    // UI Controls
    let wordTextBox = new TextBox(Location = Point(12, 12), Width = 200)
    let definitionTextBox = new TextBox(Location = Point(12, 40), 
                                      Width = 400, 
                                      Height = 100,
                                      Multiline = true)
    let searchTextBox = new TextBox(Location = Point(12, 150), Width = 200)
    let resultListBox = new ListBox(Location = Point(12, 180),
                                  Width = 400,
                                  Height = 200)

    let addButton = new Button(Text = "Add/Update",
                             Location = Point(220, 10))
    let deleteButton = new Button(Text = "Delete",
                                Location = Point(320, 10))
    let searchButton = new Button(Text = "Search",
                                Location = Point(220, 148))
    let saveButton = new Button(Text = "Save",
                              Location = Point(12, 390))
    let loadButton = new Button(Text = "Load",
                              Location = Point(100, 390))

    do
        this.Text <- "F# Dictionary"
        this.Width <- 450
        this.Height <- 480

        // Add controls to form
        this.Controls.AddRange([| 
            wordTextBox
            definitionTextBox 
            searchTextBox
            resultListBox
            addButton
            deleteButton
            searchButton
            saveButton
            loadButton
        |])

        // Event handlers
        addButton.Click.Add(fun _ ->
            if not (String.IsNullOrWhiteSpace(wordTextBox.Text)) then
                dictionary := Map.add 
                    (wordTextBox.Text.ToLower()) 
                    definitionTextBox.Text 
                    !dictionary
                MessageBox.Show("Word added/updated successfully!") |> ignore
                wordTextBox.Clear()
                definitionTextBox.Clear()
        )

        deleteButton.Click.Add(fun _ ->
            if not (String.IsNullOrWhiteSpace(wordTextBox.Text)) then
                dictionary := Map.remove (wordTextBox.Text.ToLower()) !dictionary
                MessageBox.Show("Word deleted successfully!") |> ignore
                wordTextBox.Clear()
                definitionTextBox.Clear()
        )

        searchButton.Click.Add(fun _ ->
            resultListBox.Items.Clear()
            let searchTerm = searchTextBox.Text.ToLower()
            let matches =
                !dictionary
                |> Map.filter (fun word def -> 
                    word.Contains(searchTerm))
                |> Map.toList
            
            for (word, def) in matches do
                resultListBox.Items.Add(sprintf "%s: %s" word def)
        )

        saveButton.Click.Add(fun _ ->
            let entries =
                !dictionary
                |> Map.toList
                |> List.map (fun (w, d) -> { Word = w; Definition = d })
            let json = JsonSerializer.Serialize(entries)
            File.WriteAllText(currentFile, json)
            MessageBox.Show("Dictionary saved successfully!") |> ignore
        )

        loadButton.Click.Add(fun _ ->
            try
                if File.Exists(currentFile) then
                    let json = File.ReadAllText(currentFile)
                    let entries = JsonSerializer.Deserialize<DictionaryEntry list>(json)
                    dictionary := 
                        entries
                        |> List.map (fun e -> e.Word, e.Definition)
                        |> Map.ofList
                    MessageBox.Show("Dictionary loaded successfully!") |> ignore
                else
                    MessageBox.Show("No dictionary file found!") |> ignore
            with
            | ex -> MessageBox.Show("Error loading dictionary: " + ex.Message) |> ignore
        ) 