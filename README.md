Cog
===

Cog is a tool for comparing languages using lexicostatistics and comparative linguistics techniques.
It can be used to automate much of the process of comparing word lists from different language
varieties.

Features
--------

- IPA-based segmentation: automatically splits words in to segments
- Stem identification: identifies prefixes and suffixes so that they can be ignored during
comparison
- Word alignment: aligns segments between word pairs
- Sound correspondence identification: automatically identifies sound correspondences and the
environments in which they occur
- Cognate identification: provides various methods for identifying cognates
- Lexical/phonetic similarity: calculates lexical/phonetic similarity for multiple language
varieties
- Visualization: generates similarity matrices, hierarchical graphs (UPGMA, Neighbor-joining), and
network graphs

Experimentation
---------------

The goal of Cog is to provide a framework for experimenting with different techniques for language
variety comparison. It is intended to be used iteratively: run a comparison, analyze the results,
refine the process, run the comparison again, and so on. Most steps in the process can be tailored.
It currently only supports a few comparison techniques, but we hope to include many more in the
future.