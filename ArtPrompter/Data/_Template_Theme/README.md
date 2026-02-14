To create a custom theme copy this folder and rename it to your theme/topic name.

You can then edit the prefix | subjects | suffix .json files to add you own prompt generation.

The format for the json files is as follows:

	[
	    "prompt item #1",
	    "prompt item #2"
	]

Ensure when editing manually you add a comma after each item except the last one.

Prompt generation looks like this:
[Prefix] [Subject] [Suffix]
Where each item is randomly selected from the respective json file. 
You can have as many items in each file as you like.