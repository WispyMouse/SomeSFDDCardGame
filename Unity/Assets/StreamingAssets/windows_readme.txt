Status Effects and Artifacts work identically.
They're a series of "Windows" and "Effects". For example:

{
	"Id": "poison",
	"Effects":
	[
		{
			"Window": "SELF_STARTTURN",
			"Effect": "[SETTARGET: SELF][DAMAGE: 1]"
		}
	]
}

A status effect can have any number of windows. You can also have multiple effects on the same window, and they'll be applied in order.
Whenever a Window occurs, there's a set of contextual information.

# Window Glossary

SELF_STARTTURN
- SELF is the entity with the status effect on it