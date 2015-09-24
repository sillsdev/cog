cog-cmdline.exe(1) -- Use Cog features from the Linux command line
==================================================================

## SYNOPSIS

`mono cog-cmdline.exe` (operation) [parameters]

## EXAMPLES

`mono cog-cmdline.exe` syllabify
`mono cog-cmdline.exe` pairs
`mono cog-cmdline.exe` alignment -n
`mono cog-cmdline.exe` cluster dbscan -e 0.2 -m 3
`mono cog-cmdline.exe` cluster lsdbc -a 4 -k 5
`mono cog-cmdline.exe` cluster upgma -t 0.2

## DESCRIPTION

Operations currently defined:

 + `syllabify`
 + `pairs`
 + `alignment`
 + `cluster`

Each operation's output can be chained into the input of the next operation. For example, if you have a UTF-8 text file called `words.txt` containing these words:

    cat
    call
    bat
    ball
    kill

You can run:

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe pairs | mono cog-cmdline.exe alignment -n | mono cog-cmdline.exe cluster upgma -t 0.2`

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

The `pairs` operation is designed to be used as input to the `alignment` operation. It takes a
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

This output can then be piped into the `alignment` operation. Alternately, it could be chopped into
any number of segments that could each be passed to a `alignment` operation on a separate machine,
if you want parallel processing.

### Alignment measurement

The alignment measurement expects two words per line, and each line of output will be the two input
words plus a score (either an integer or a real number, depending on whether raw or normalized scores
were chosen). See [OPTIONS](#OPTIONS) for more on how to specify raw or normalized scores. (Summary:
`-r` or `-n`).

With raw scores, the output of the above `pairs` operation will be:

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

Note that the stem separators (`|`) have vanished in the output of `alignment`. That's because they
are not needed in the `cluster` operation, which only cares about the scores.

The `--verbose` or `-v` output from `alignment` looks a little different: below each "word1 word2 score"
line will be two more lines showing the alignment of the two words that was used to calculate their
similarity score, followed by a blank line to visually separate each word pair. For example, the first
few lines of our sample text would look like this if `alignment` was run with `-n` and `-v`:

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

The `cluster` operation expects input in the format produced by the `alignment` operation without `-v`,
e.g. two words per line, with a numeric score after the two words. It takes several parameters: the
`-m` or `--method` parameter specifies the clustering method (DBSCAN, LSDBC, or UPGMA) and the other
parameters are used for adjusting the clustering method. See [OPTIONS](#OPTIONS) for details, but the
gist is that depending on the clustering parameters, you can produce either "looser" clustering, where
more words will end up in the same cluster, or "stricter" clustering, where more clusters will be
produced with fewer words in each. Thus, the `-t` or `--threshhold` parameter (used with UPGMA) will
produce stricter clustering when it is lower, but looser clustering when it is higher:

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe pairs | mono cog-cmdline.exe alignment -n | mono cog-cmdline.exe cluster upgma -t 0.2` produces:

    1 bal.l call. kill.
    2 cat bat

`cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe pairs | mono cog-cmdline.exe alignment -n | mono cog-cmdline.exe cluster upgma -t 0.19` produces:

    1 cat
    2 bat
    3 bal.l call. kill.

And `cat words.txt | mono cog-cmdline.exe syllabify | mono cog-cmdline.exe pairs | mono cog-cmdline.exe alignment -n | mono cog-cmdline.exe cluster upgma -t 0.18` produces:

    1 cat
    2 bat
    3 bal.l
    4 call. kill.

Similar effects (more or fewer words per cluster) can be achieved by tweaking the numeric parameters of
the other two clustering methods; examples omitted to save space.

**NOTE** that `cluster` cannot be run in parallel, as it needs to have the complete list of words and alignments
in order to calculate the clustering.

## OPTIONS

### Common options:

 * `-i` FNAME, `--input`=FNAME:
   Take input from FNAME instead of stdin (to explicitly specify stdin, use `-` for FNAME)
 * `-o` FNAME, `--output`=FNAME:
   Write output to FNAME instead of stdin (to explicitly specify stdout, use `-` for FNAME)
 * `-c` CONFIG_FILE, `--config-file`=CONFIG_FILE:
   Configuration file to use instead of the default. By default, config files will be searched for in the following order:

     1. The file specified with `-c` or `--config-file`, if any
     1. `$HOME/.config/cog-cmdline/cog-cmdline.conf`
     1. `/usr/share/cog-cmdline/cog-cmdline.conf`

 * `--config-data`=STRING:
   Configuration data to use instead of a config file; takes precedence over `--config-file` if both are specified. The entire contents of Cog's config file (which is in XML format) will be expected as a single parameter; since this XML data will likely contain both double-quote and single-quote characters, properly quoting that parameter is left as an exercise for the user.

### Operation-specific options:

### Syllabify

No options specific to the `syllabify` command.

### Pairs

No options specific to the `pairs` command.

### Alignment

 + `-n`, `--normalized-scores`:
   Produce normalized scores (real numbers between 0.0 and 1.0, where 1.0 is a perfect match)
 + `-r`, `--raw-scores`:
   Produce raw scores (integers between 0 and infinity, where higher means a better match but
   there is no specific score that means "perfect match")
 + `-v`, `--verbose`:
   Be more verbose, showing each word's alignment (this changes the output so that it's no longer
   suitable for piping directly into the `cluster` operation, but the output could still be parsed
   using a script).

Note that `-r` may not work well for passing into the `cluster` operation, as `cluster`'s results tend to be better if its input is normalized. Therefore `-n` is the default; if no scoring method is specified, the `alignment` operation will default to normalized scores (and will produce a warning so that you know it has made this choice).

### Cluster

 + `-m` METHOD, `--method`=METHOD:
   Clustering method. Valid values are `dbscan`, `lsdbc` and `upgma`. Case-insensitive: "UPGMA", "Upgma", and "upgma" are all the same method. Default: `upgma`.

The rest of the possible options for clustering depend on the method.

DBSCAN options:

 + `-e` NUM, `--epsilon`=NUM:
   Epsilon value for DBSCAN clustering. Words must be within distance "epsilon" of each other to be considered for a cluster. Should be a real number between 0.0 and 1.0, where higher means "easier clustering". A threshhold of 1 will produce a single cluster containing every word in the input, while a threshhold of 0 will produce as many clusters as there are input words, with each cluster containing a single word. Default is 0.2, as that seems to produce somewhat reasonable clustering results.
 + `-M` NUM, `--min-words`=NUM:
   Minimum number of "close" words to form the core of a cluster in DBSCAN clustering. If this value is higher, clusters will be harder to form, and more words will be left outside clusters (more "clusters" of just one word will appear in the output). A lower value will make clusters easier to form, and fewer words will tend to remain unclustered. Default is 2.

LSDBC options:

 + `-a` NUM, `--alpha`=NUM:
   Alpha value for LSDBC clustering. Should be a real number between 0.0 and infinity, where higher means "easier clustering". A value of 0 will tend to produce more clusters with fewer words per cluster, while higher values tend towards very few clusters with many words in each cluster. Default is 0.2.
 + `-k` NUM:
   How many neighbors of each word to consider while clustering. (The `K` value in LSDBC's "K nearest neighbors" algorithm). Should be an integer between 0 and infinity, but values of 0 or 1 tend to produce nothing but 1-word or 2-word clusters, so a value of 2 or more is recommended. Default is 3.

UPGMA options:

 + `-t` NUM, `--threshhold`=NUM:
   Threshhold for UPGMA clustering: words further apart than the threshhold value will not be clustered together. Should be a real number between 0.0 and 1.0, where higher means "easier clustering". A threshhold of 1 will produce a single cluster containing every word in the input, while a threshhold of 0 will produce as many clusters as there are input words, with each cluster containing a single word. Default is 0.2, as that seems to produce somewhat reasonable clustering results.
