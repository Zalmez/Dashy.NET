import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';

import styles from './index.module.css';

type DocPath = {
  title: string;
  description: string;
  to: string;
};

const docPaths: DocPath[] = [
  {
    title: 'Tutorials',
    description: 'Follow guided lessons to get Dashy running and understand the basics.',
    to: '/docs/tutorials/first-local-run',
  },
  {
    title: 'How-To Guides',
    description: 'Solve specific tasks quickly, from local run workflows to OIDC setup.',
    to: '/docs/how-to/run-dashy-locally',
  },
  {
    title: 'Reference',
    description: 'Look up endpoint indexes, authorization rules, and implementation details.',
    to: '/docs/reference/api-endpoint-index',
  },
  {
    title: 'Explanation',
    description: 'Understand architecture, identity model decisions, and system behavior.',
    to: '/docs/explanation/system-architecture',
  },
];

function Hero() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx(styles.heroBanner)}>
      <div className="container">
        <Heading as="h1" className={styles.title}>
          {siteConfig.title}
        </Heading>
        <p className={styles.subtitle}>{siteConfig.tagline}</p>
        <p className={styles.description}>
          Documentation for operators, contributors, and integrators building with Dashy.
        </p>
        <div className={styles.ctaRow}>
          <Link
            className="button button--primary button--lg"
            to="/docs/intro">
            Open Documentation
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs/tutorials/first-local-run">
            Start First Tutorial
          </Link>
          <Link
            className="button button--outline button--lg"
            to="https://github.com/Zalmez/Dashy.NET">
            View Repository
          </Link>
        </div>
      </div>
    </header>
  );
}

function PathsSection() {
  return (
    <section className={styles.pathsSection}>
      <div className="container">
        <Heading as="h2" className={styles.sectionTitle}>
          Choose Your Path
        </Heading>
        <div className={styles.pathGrid}>
          {docPaths.map((path) => (
            <Link key={path.title} className={styles.pathCard} to={path.to}>
              <Heading as="h3" className={styles.pathTitle}>
                {path.title}
              </Heading>
              <p className={styles.pathDescription}>{path.description}</p>
              <span className={styles.pathAction}>Open section</span>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}

function StackSection() {
  return (
    <section className={styles.stackSection}>
      <div className="container">
        <Heading as="h2" className={styles.sectionTitle}>
          Platform Snapshot
        </Heading>
        <div className={styles.stackRow}>
          <div className={styles.stackItem}>.NET Aspire Orchestration</div>
          <div className={styles.stackItem}>Blazor Web Frontend</div>
          <div className={styles.stackItem}>Minimal API Backend</div>
          <div className={styles.stackItem}>PostgreSQL + Redis</div>
        </div>
      </div>
    </section>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={siteConfig.title}
      description="Dashy documentation landing page">
      <Hero />
      <main>
        <PathsSection />
        <StackSection />
      </main>
    </Layout>
  );
}
