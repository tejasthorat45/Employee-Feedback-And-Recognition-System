import { Injectable } from '@angular/core';

export type SentimentType = 'positive' | 'negative' | 'neutral';

export interface SentimentResult {
  sentiment: SentimentType;
  score: number; // -1 to 1 scale
  confidence: number; // 0 to 1
}

@Injectable({ providedIn: 'root' })
export class SentimentDeciderService {
  
  // Positive keywords and phrases
  private readonly positiveKeywords = [
    'excellent', 'great', 'good', 'outstanding', 'fantastic', 'amazing', 'wonderful',
    'impressive', 'superb', 'brilliant', 'exceptional', 'perfect', 'awesome', 'terrific',
    'helpful', 'professional', 'reliable', 'efficient', 'dedicated', 'skilled', 'talented',
    'innovative', 'creative', 'proactive', 'collaborative', 'supportive', 'motivated',
    'hardworking', 'committed', 'consistent', 'thorough', 'detail-oriented', 'well done',
    'keep up', 'exceeded', 'strength', 'improve', 'appreciate', 'thank', 'grateful',
    'strong', 'effective', 'successful', 'achieve', 'accomplish', 'deliver', 'quality'
  ];

  // Negative keywords and phrases
  private readonly negativeKeywords = [
    'poor', 'bad', 'terrible', 'awful', 'horrible', 'disappointing', 'unacceptable',
    'inadequate', 'unsatisfactory', 'subpar', 'mediocre', 'lacking', 'insufficient',
    'fail', 'failed', 'failure', 'mistake', 'error', 'problem', 'issue', 'concern',
    'weak', 'slow', 'careless', 'unprofessional', 'unreliable', 'inconsistent',
    'late', 'missed', 'ignore', 'neglect', 'difficult', 'struggle', 'unable',
    'not good', 'not effective', 'not acceptable', 'needs improvement', 'below expectations',
    'rude', 'disorganized', 'lazy', 'unmotivated', 'incomplete', 'sloppy'
  ];

  // Neutral/constructive keywords
  private readonly neutralKeywords = [
    'consider', 'suggest', 'recommend', 'could', 'would', 'should', 'maybe',
    'perhaps', 'opportunity', 'potential', 'develop', 'learn', 'grow', 'training',
    'average', 'adequate', 'acceptable', 'meets', 'standard', 'typical', 'normal',
    'room for', 'continue', 'maintain', 'observe', 'note', 'review'
  ];

  // Intensifiers that strengthen sentiment
  private readonly intensifiers = [
    'very', 'really', 'extremely', 'absolutely', 'totally', 'completely',
    'highly', 'incredibly', 'remarkably', 'exceptionally', 'particularly'
  ];

  // Negators that reverse sentiment
  private readonly negators = [
    'not', 'no', 'never', 'neither', 'nor', 'nothing', 'nobody', 'none',
    'hardly', 'scarcely', 'barely', 'rarely', 'seldom', 'cannot', 'can\'t',
    'won\'t', 'wouldn\'t', 'shouldn\'t', 'couldn\'t', 'don\'t', 'doesn\'t', 'didn\'t'
  ];

  constructor() {}

  /**
   * Analyze text and determine sentiment
   */
  analyzeSentiment(text: string): SentimentResult {
    if (!text || text.trim().length === 0) {
      return { sentiment: 'neutral', score: 0, confidence: 0 };
    }

    const normalizedText = text.toLowerCase().trim();
    const words = this.tokenize(normalizedText);
    
    let positiveScore = 0;
    let negativeScore = 0;
    let neutralScore = 0;
    let totalMatches = 0;

    for (let i = 0; i < words.length; i++) {
      const word = words[i];
      const prevWord = i > 0 ? words[i - 1] : '';
      const hasNegator = this.negators.includes(prevWord);
      const hasIntensifier = this.intensifiers.includes(prevWord);
      const multiplier = hasIntensifier ? 1.5 : 1.0;

      // Check positive keywords
      if (this.positiveKeywords.includes(word)) {
        if (hasNegator) {
          negativeScore += 1 * multiplier;
        } else {
          positiveScore += 1 * multiplier;
        }
        totalMatches++;
      }

      // Check negative keywords
      if (this.negativeKeywords.includes(word)) {
        if (hasNegator) {
          positiveScore += 1 * multiplier;
        } else {
          negativeScore += 1 * multiplier;
        }
        totalMatches++;
      }

      // Check neutral keywords
      if (this.neutralKeywords.includes(word)) {
        neutralScore += 1;
        totalMatches++;
      }
    }

    // Check for multi-word phrases
    const phrasesScore = this.checkPhrases(normalizedText);
    positiveScore += phrasesScore.positive;
    negativeScore += phrasesScore.negative;
    neutralScore += phrasesScore.neutral;
    totalMatches += phrasesScore.matches;

    // Calculate final score and sentiment
    const netScore = positiveScore - negativeScore;
    const totalScore = positiveScore + negativeScore + neutralScore;
    
    // Normalize score to -1 to 1 range
    const normalizedScore = totalScore > 0 ? netScore / totalScore : 0;
    
    // Calculate confidence based on number of matches
    const confidence = Math.min(totalMatches / 5, 1.0); // Max confidence at 5+ matches

    // Determine sentiment type
    let sentiment: SentimentType;
    if (normalizedScore > 0.2) {
      sentiment = 'positive';
    } else if (normalizedScore < -0.2) {
      sentiment = 'negative';
    } else {
      sentiment = 'neutral';
    }

    return {
      sentiment,
      score: normalizedScore,
      confidence
    };
  }

  /**
   * Batch analyze multiple texts
   */
  analyzeBatch(texts: string[]): SentimentResult[] {
    return texts.map(text => this.analyzeSentiment(text));
  }

  /**
   * Get sentiment statistics from multiple texts
   */
  getSentimentStats(texts: string[]): {
    positive: number;
    negative: number;
    neutral: number;
    total: number;
  } {
    const results = this.analyzeBatch(texts);
    
    return {
      positive: results.filter(r => r.sentiment === 'positive').length,
      negative: results.filter(r => r.sentiment === 'negative').length,
      neutral: results.filter(r => r.sentiment === 'neutral').length,
      total: results.length
    };
  }

  /**
   * Tokenize text into words
   */
  private tokenize(text: string): string[] {
    return text
      .toLowerCase()
      .replace(/[^\w\s'-]/g, ' ')
      .split(/\s+/)
      .filter(word => word.length > 0);
  }

  /**
   * Check for multi-word phrases
   */
  private checkPhrases(text: string): {
    positive: number;
    negative: number;
    neutral: number;
    matches: number;
  } {
    const positivePhrases = [
      'well done', 'good job', 'keep up', 'great work', 'excellent work',
      'outstanding performance', 'exceeded expectations', 'goes above and beyond',
      'highly recommend', 'strong performance', 'valuable contribution'
    ];

    const negativePhrases = [
      'needs improvement', 'not acceptable', 'below expectations', 'room for improvement',
      'could be better', 'falls short', 'not satisfactory', 'major concern',
      'significantly lacking', 'poor performance', 'failed to meet'
    ];

    const neutralPhrases = [
      'room for growth', 'could improve', 'consider reviewing', 'opportunity to',
      'meets expectations', 'average performance', 'adequate performance'
    ];

    let positive = 0;
    let negative = 0;
    let neutral = 0;
    let matches = 0;

    positivePhrases.forEach(phrase => {
      if (text.includes(phrase)) {
        positive += 1.5;
        matches++;
      }
    });

    negativePhrases.forEach(phrase => {
      if (text.includes(phrase)) {
        negative += 1.5;
        matches++;
      }
    });

    neutralPhrases.forEach(phrase => {
      if (text.includes(phrase)) {
        neutral += 1;
        matches++;
      }
    });

    return { positive, negative, neutral, matches };
  }
}
