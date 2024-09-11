Status Effects and Artifacts work identically.
They're a series of "Windows" and "Effects". For example:

{
	"Id": "poison",
	"Effects":
	[
		{
			"Window": "OWNER_STARTTURN",
			"Effect": "[SETTARGET: OWNER][DAMAGE: 1][DECREASESTACK]"
		}
	]
}

A status effect can have any number of windows. You can also have multiple effects on the same window, and they'll be applied in order.
Whenever a Window occurs, there's a set of contextual information.

# Window Glossary

OWNER_STARTTURN
- OWNER is the entity with the status effect on it