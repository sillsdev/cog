cog-cmdline.exe(1) -- Use Cog features from the Linux command line
==================================================================

## SYNOPSIS

`mono cog-cmdline.exe` (operation) [parameters]

## EXAMPLES

`mono cog-cmdline.exe` syllabify  
`mono cog-cmdline.exe` make-pairs  
`mono cog-cmdline.exe` distance -n  
`mono cog-cmdline.exe` cluster -t 0.2  

## DESCRIPTION

Operations currently defined:

 + `syllabify`
 + `make-pairs`
 + `distance`
 + `cluster`

Each operation's output can be chained into the input of the next operation. For example, if you have a UTF-8 text file called `words.txt` containing these words:

    cat
    call
    bat
    ball
    kill

You can run:

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe make-pairs | mono cog-cmdline.exe distance -n | mono cog-cmdline.exe cluster -t 0.2`

And the output should be:

    1 bal.l cal.l kil.l
    2 cat bat

### Syllabification

The `syllabify` operation expects its input to contain one word per line, and outputs one word
per line with syllables marked by periods (`.`) between each syllable. It also tries to identify
word stems, and marks the stem by using pipe characters (`|`) before and after the stem.

Note that the `syllabify` operation was designed for IPA input. So with our sample input in the Latin alphabet,
it has gotten the syllabification wrong in some words. To correct that, you can pass in input that has been
pre-syllabified with periods between syllables. If a word in the input contains periods, `syllabify` will use
that pre-existing syllabification for that word instead of its own. Any words in the input that do not contain
periods will still be syllabified. Thus if `words.txt` was:

    cat
    call.
    bat
    ball
    kill.

The `syllabify` output will be:

    |cat|
    |call.|
    |bat|
    |bal.l|
    |kill.|

### Making pairs

The `make-pairs` operation is designed to be used as input to the `distance` operation. It takes a
list of words, one word per line, as its input, and produces as output a list of every pairwise
combination of those words. Thus the input:

    |cat|
    |call.|
    |bat|
    |bal.l|
    |kill.|

will produce:

    |cat| |call.|
    |cat| |bat|
    |cat| |bal.l|
    |cat| |kill.|
    |call.| |bat|
    |call.| |bal.l|
    |call.| |kill.|
    |bat| |bal.l|
    |bat| |kill.|
    |bal.l| |kill.|

This output can then be piped into the `distance` operation. Alternately, it could be chopped into
any number of segments that could each be passed to a `distance` operation on a separate machine,
if you want parallel processing.

### Distance measurement

The distance measurement expects two words per line, and each line of output will be the two input
words plus a score (either an integer or a real number, depending on whether raw or normalized scores
were chosen). See [OPTIONS](#OPTIONS) for more on how to specify raw or normalized scores. (Summary:
`-r` or `-n`).

With raw scores, the output of the above `make-pairs` operation will be:

    cat call. 0.535714285714286
    cat bat 0.8
    cat bal.l 0.385714285714286
    cat kill. 0.435714285714286
    call. bat 0.385714285714286
    call. bal.l 0.85
    call. kill. 0.9
    bat bal.l 0.535714285714286
    bat kill. 0.285714285714286
    bal.l kill. 0.75

Note that the stem separators (`|`) have vanished in the output of `distance`. That's because they
are not needed in the `cluster` operation, which only cares about the scores.

The `--verbose` or `-v` output from `distance` looks a little different: below each "word1 word2 score"
line will be two more lines showing the alignment of the two words that was used to calculate their
similarity score, followed by a blank line to visually separate each word pair. For example, the first
few lines of our sample text would look like this if `distance` was run with `-n` and `-v`:

    cat call. 0.535714285714286
    |c a - t|
    |c a l l|
    
    cat bat 0.8
    |c a t|
    |b a t|
    
    cat bal.l 0.385714285714286
    |c a - t|
    |b a l l|
    
    (etc., etc., etc.)

### Clustering

The `cluster` operation expects input in the format produced by the `distance` operation without `-v`,
e.g. two words per line, with a numeric score after the two words. It takes a `--threshhold` or `-t`
parameter with which you can tweak clustering parameters. See [OPTIONS](#OPTIONS) for details, but the gist is
that a higher threshhold produces "looser" clustering, where more words will end up in the same cluster.
Lower threshhold values produce "stricter" clustering, where more clusters will be produced with fewer
words in each. Thus:

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe make-pairs | mono cog-cmdline.exe distance -n | mono cog-cmdline.exe cluster -t 0.2` produces:

    1 bal.l call. kill.
    2 cat bat

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe make-pairs | mono cog-cmdline.exe distance -n | mono cog-cmdline.exe cluster -t 0.19` produces:

    1 cat
    2 bat
    3 bal.l call. kill.

And `cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe make-pairs | mono cog-cmdline.exe distance -n | mono cog-cmdline.exe cluster -t 0.18` produces:

    1 cat
    2 bat
    3 bal.l
    4 call. kill.

Note that `cluster` cannot be run in parallel, as it needs to have the complete list of words and distances
in order to calculate the clustering.

## OPTIONS

### Common options:

 * `-i` FNAME, `--input`=FNAME:
   Take input from FNAME instead of stdin (to explicitly specify stdin, use `-` for FNAME)
 * `-o` FNAME, `--output`=FNAME:
   Write output to FNAME instead of stdin (to explicitly specify stdout, use `-` for FNAME)
   
### Operation-specific options:

### Syllabify

No options specific to the `syllabify` command.

### Make-pairs

No options specific to the `make-pairs` command.

### Distance

 + `-n`, `--normalized-scores`:
   Produce normalized scores (real numbers between 0.0 and 1.0, where 1.0 is a perfect match)
 + `-r`, `--raw-scores`:
   Produce raw scores (integers between 0 and infinity, where higher means a better match but
   there is no specific score that means "perfect match")
 + `-v`, `--verbose`:
   Be more verbose, showing each word's alignment (this changes the output so that it's no longer
   suitable for piping directly into the `cluster` operation, but the output could still be parsed
   using a script).

Note that `-r` may not work well for passing into the `cluster` operation, as `cluster`'s results tend to be better if its input is normalized. Therefore `-n` is the default; if no scoring method is specified, the `distance` operation will default to normalized scores (and will produce a warning so that you know it has made this choice).

### Cluster

 + `-t` NUM, `--threshhold`=NUM:
   Threshhold for clustering operation. Should be a real number between 0.0 and 1.0, where higher means "easier clustering". A threshhold of 1 will produce a single cluster containing every word in the input, while a threshhold of 0 will produce as many clusters as there are input words, with each cluster containing a single word. Default is 0.2, as that seems to produce somewhat reasonable clustering results.