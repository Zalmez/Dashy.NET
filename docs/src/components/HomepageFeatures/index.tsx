import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Built with .NET',
    Svg: require('@site/static/img/builtwith.svg').default,
    description: (
      <>
        Dashy.NET is built entirely on the .NET platform using Blazor for the frontend 
        and .NET Aspire for orchestration, providing a modern, performant, and 
        cross-platform dashboarding experience.
      </>
    ),
  },
  {
    title: 'Easy Configuration',
    Svg: require('@site/static/img/easyconfig.svg').default,
    description: (
      <>
        Configure your entire dashboard through an intuitive web interface. 
        No YAML files or complex configuration required - just point, click, and customize.
      </>
    ),
  },
  {
    title: 'Widget System',
    Svg: require('@site/static/img/widgetsystem.svg').default,
    description: (
      <>
        Display dynamic information with built-in widgets like real-time clocks, 
        weather forecasts, and system monitors. Easily extensible for custom widgets.
      </>
    ),
  },
];

function Feature({title, Svg, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
