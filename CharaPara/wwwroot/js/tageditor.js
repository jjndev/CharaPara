class tagEditor {


    constructor() {
        this.searchDatabase();

        const hashtagList = document.getElementById('hashtagList');

        
    }

    initialize() {
        const hashtagList = document.getElementById('hashtagList');

        let tags = {};

        let isInitialized = false;

        document.addEventListener('DOMContentLoaded', function () {
            //store values from the searchTagIdString into an array
            var searchTagIdString = document.getElementById("searchTagIdString").value;
            var searchTagIds = searchTagIdString.split(";");
        }
        
    }

    updateTags() {
        tags.forEach(hashtag => {
            const listItem = document.createElement('span');
            listItem.className = 'hashtag';
            listItem.id = `hashtag-${hashtag.id}`;

            const label = document.createElement('span');
            label.innerText = hashtag.name + " ";

            const deleteButton = document.createElement('div');
            deleteButton.className = 'x';
            deleteButton.innerText = 'x';

            deleteButton.addEventListener('click', () => {
                hashtagList.removeChild(listItem);
                tags = tags.filter(tag => tag.id !== hashtag.id);
            });

            listItem.appendChild(label);
            label.appendChild(deleteButton);

            hashtagList.appendChild(listItem);

        });
    }


    searchDatabase() {
        var searchQuery = document.getElementById('searchBox').value;
        fetch('/SearchForTag?q=' + searchQuery)
            .then(response => response.json())
            .then(data => {
                var resultsContainer = document.getElementById('searchResults');
                resultsContainer.innerHTML = ''; // Clear previous results
                data.forEach(item => {
                    // Assuming item has 'Name' property to display
                    resultsContainer.innerHTML += `<span class="badge badge-secondary" name="${item.description}">${item.name}</span>`;
                });
            });
    }

    
}

const tags = [
    { id: 1, name: '#test1', description: 'This is a test hashtag #1' },
    { id: 2, name: '#test2', description: 'This is a test hashtag #2' },
    { id: 3, name: '#test3', description: 'This is a test hashtag #3' }
];




const lastHashtag = document.getElementById(`hashtag-${tags[tags.length - 1].id}`);
const form = lastHashtag.parentNode;
form.style.display = 'inline';
form.style.marginLeft = '1em';





